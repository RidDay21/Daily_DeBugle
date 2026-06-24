using System.Collections.Concurrent;
using System.Text;
using DailyDeBugle.Models;
using DailyDeBugle.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DailyDeBugle.Services
{
    public interface ICollaborativeEditingService
    {
        Task<(string Content, int Revision)> GetDocumentAsync(int articleId);
        Task ApplyOperationAsync(int articleId, TextOperation operation, string connectionId);
        void SetDocument(int articleId, string content);
        void RemoveDocument(int articleId);
        int GetActiveEditorCount(int articleId);
        IReadOnlyList<string> GetActiveEditorNames(int articleId);
    }

    public class CollaborativeEditingService : ICollaborativeEditingService
    {
        private readonly ConcurrentDictionary<int, DocumentState> _documents = new();
        private readonly IHubContext<EditingHub> _hubContext;

        // Хранилище активных редакторов: articleId -> список (connectionId, userName)
        // Используется из EditingHub напрямую; доступ через методы этого сервиса.
        internal readonly ConcurrentDictionary<int, List<EditorEntry>> _activeEditors = new();
        private readonly object _editorsLock = new();

        public CollaborativeEditingService(IHubContext<EditingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Документ
        // ─────────────────────────────────────────────────────────────────────

        public Task<(string Content, int Revision)> GetDocumentAsync(int articleId)
        {
            if (_documents.TryGetValue(articleId, out var state))
            {
                lock (state.Lock)
                {
                    return Task.FromResult((state.Content.ToString(), state.Revision));
                }
            }
            return Task.FromResult((string.Empty, 0));
        }

        public void SetDocument(int articleId, string content)
        {
            // Создаём или сбрасываем документ только если он ещё не инициализирован
            // (первый вошедший редактор задаёт содержимое).
            _documents.AddOrUpdate(
                articleId,
                _ => new DocumentState { Content = new StringBuilder(content), Revision = 0 },
                (_, existing) =>
                {
                    // Документ уже существует — не перезаписываем чужие правки
                    return existing;
                });
        }

        /// <summary>
        /// Принудительно сбросить документ (вызывается при финальном сохранении статьи).
        /// </summary>
        public void RemoveDocument(int articleId)
        {
            _documents.TryRemove(articleId, out _);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Применение и трансформация операции
        // ─────────────────────────────────────────────────────────────────────

        public async Task ApplyOperationAsync(int articleId, TextOperation operation, string connectionId)
        {
            if (!_documents.TryGetValue(articleId, out var doc))
                return;

            TextOperation transformed;
            int newRevision;

            lock (doc.Lock)
            {
                // Трансформируем операцию относительно всех операций, выполненных
                // после той ревизии, на основе которой она была создана.
                transformed = TransformAgainstHistory(operation, doc);

                // Клампируем позицию и длину к реальным границам документа
                transformed = ClampOperation(transformed, doc.Content.Length);

                // Применяем к серверному документу
                ApplyToStringBuilder(doc.Content, transformed);

                doc.Revision++;
                newRevision = doc.Revision;

                // Сохраняем в историю (храним не более 1000 операций)
                transformed.Revision = newRevision;
                doc.History.Add(transformed);
                if (doc.History.Count > 1000)
                    doc.History.RemoveAt(0);
            }

            // Рассылаем трансформированную операцию ВСЕМ клиентам группы
            // (включая отправителя — чтобы он обновил свою локальную ревизию).
            await _hubContext.Clients
                .Group($"doc-{articleId}")
                .SendAsync("OperationApplied", transformed, newRevision, connectionId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Активные редакторы
        // ─────────────────────────────────────────────────────────────────────

        public int GetActiveEditorCount(int articleId)
        {
            lock (_editorsLock)
            {
                return _activeEditors.TryGetValue(articleId, out var list) ? list.Count : 0;
            }
        }

        public IReadOnlyList<string> GetActiveEditorNames(int articleId)
        {
            lock (_editorsLock)
            {
                if (!_activeEditors.TryGetValue(articleId, out var list))
                    return Array.Empty<string>();
                return list.Select(e => e.UserName).Distinct().ToArray();
            }
        }

        internal void AddEditor(int articleId, string connectionId, string userName)
        {
            lock (_editorsLock)
            {
                var list = _activeEditors.GetOrAdd(articleId, _ => new List<EditorEntry>());
                // Не добавляем дубликаты по connectionId
                if (!list.Any(e => e.ConnectionId == connectionId))
                    list.Add(new EditorEntry(connectionId, userName));
            }
        }

        internal void RemoveEditor(int articleId, string connectionId)
        {
            lock (_editorsLock)
            {
                if (_activeEditors.TryGetValue(articleId, out var list))
                {
                    list.RemoveAll(e => e.ConnectionId == connectionId);
                    if (list.Count == 0)
                        _activeEditors.TryRemove(articleId, out _);
                }
            }
        }

        /// <summary>
        /// Удаляет редактора из всех документов (при разрыве соединения).
        /// Возвращает список articleId, из которых он был удалён.
        /// </summary>
        internal List<int> RemoveEditorFromAll(string connectionId)
        {
            var affected = new List<int>();
            lock (_editorsLock)
            {
                foreach (var kvp in _activeEditors)
                {
                    int before = kvp.Value.Count;
                    kvp.Value.RemoveAll(e => e.ConnectionId == connectionId);
                    if (kvp.Value.Count != before)
                    {
                        affected.Add(kvp.Key);
                        if (kvp.Value.Count == 0)
                            _activeEditors.TryRemove(kvp.Key, out _);
                    }
                }
            }
            return affected;
        }

        // ─────────────────────────────────────────────────────────────────────
        // OT — ядро трансформации
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Трансформирует <paramref name="op"/> против всех операций из истории,
        /// начиная с индекса op.Revision (т.е. операций, выполненных после создания op).
        /// </summary>
        private static TextOperation TransformAgainstHistory(TextOperation op, DocumentState doc)
        {
            // Стартовый индекс в истории — это op.Revision.
            // История хранит операции с накопленными ревизиями 1..N.
            // Индекс в списке = ревизия - 1, если история не обрезалась.
            // Для устойчивости ищем первую операцию с Revision > op.Revision.
            int startIdx = doc.History.FindIndex(h => h.Revision > op.Revision);
            if (startIdx < 0)
                return op; // Нет более новых операций — трансформация не нужна

            var current = op;
            for (int i = startIdx; i < doc.History.Count; i++)
            {
                current = TransformPair(current, doc.History[i]);
                if (current == null!)
                {
                    // Операция полностью поглощена — возвращаем no-op
                    return new TextOperation
                    {
                        Type = "noop",
                        Position = 0,
                        UserId = op.UserId,
                        Revision = op.Revision
                    };
                }
            }
            return current;
        }

        /// <summary>
        /// Трансформирует операцию <paramref name="a"/> относительно уже применённой
        /// операции <paramref name="b"/>. Реализует классическую OT IT(a, b).
        /// </summary>
        private static TextOperation TransformPair(TextOperation a, TextOperation b)
        {
            // ── INSERT vs INSERT ──────────────────────────────────────────────
            if (a.Type == "insert" && b.Type == "insert")
            {
                int pos = a.Position;
                if (b.Position < a.Position)
                    pos += b.Text!.Length;
                else if (b.Position == a.Position)
                    // Конвенция: операция с меньшим userId «побеждает» и вставляется первой.
                    // Если userId равны — порядок по connectionId не важен; сдвигаем a вправо.
                    pos += b.Text!.Length;
                // b.Position > a.Position → a не сдвигается

                return new TextOperation
                {
                    Type = "insert",
                    Position = pos,
                    Text = a.Text,
                    UserId = a.UserId,
                    Revision = a.Revision
                };
            }

            // ── INSERT vs DELETE ──────────────────────────────────────────────
            if (a.Type == "insert" && b.Type == "delete")
            {
                int pos = a.Position;
                if (b.Position + b.Length <= a.Position)
                {
                    // Удаление полностью левее вставки
                    pos -= b.Length;
                }
                else if (b.Position < a.Position)
                {
                    // Удаление захватывает позицию вставки — вставляем в начало удалённого диапазона
                    pos = b.Position;
                }
                // b.Position >= a.Position → вставка не сдвигается

                return new TextOperation
                {
                    Type = "insert",
                    Position = pos,
                    Text = a.Text,
                    UserId = a.UserId,
                    Revision = a.Revision
                };
            }

            // ── DELETE vs INSERT ──────────────────────────────────────────────
            if (a.Type == "delete" && b.Type == "insert")
            {
                int pos = a.Position;
                int len = a.Length;

                if (b.Position <= a.Position)
                {
                    // Вставка левее или в начале удаления — сдвигаем удаление вправо
                    pos += b.Text!.Length;
                }
                else if (b.Position < a.Position + a.Length)
                {
                    // Вставка внутри удаляемого диапазона — расширяем длину,
                    // чтобы удалить и вставленный текст тоже.
                    len += b.Text!.Length;
                }
                // b.Position >= a.Position + a.Length → удаление не меняется

                return new TextOperation
                {
                    Type = "delete",
                    Position = pos,
                    Length = len,
                    UserId = a.UserId,
                    Revision = a.Revision
                };
            }

            // ── DELETE vs DELETE ──────────────────────────────────────────────
            if (a.Type == "delete" && b.Type == "delete")
            {
                int aStart = a.Position;
                int aEnd = a.Position + a.Length;
                int bStart = b.Position;
                int bEnd = b.Position + b.Length;

                // Нет перекрытия: b полностью левее a
                if (bEnd <= aStart)
                {
                    return new TextOperation
                    {
                        Type = "delete",
                        Position = aStart - b.Length,
                        Length = a.Length,
                        UserId = a.UserId,
                        Revision = a.Revision
                    };
                }

                // Нет перекрытия: b полностью правее a
                if (bStart >= aEnd)
                {
                    return new TextOperation
                    {
                        Type = "delete",
                        Position = aStart,
                        Length = a.Length,
                        UserId = a.UserId,
                        Revision = a.Revision
                    };
                }

                // Частичное или полное перекрытие — вырезаем пересечение из a
                int newStart = Math.Min(aStart, bStart);
                int overlapStart = Math.Max(aStart, bStart);
                int overlapEnd = Math.Min(aEnd, bEnd);
                int overlapLen = overlapEnd - overlapStart;
                int newLen = a.Length - overlapLen;

                if (newLen <= 0)
                {
                    // a полностью поглощена b — no-op
                    return new TextOperation
                    {
                        Type = "noop",
                        Position = 0,
                        UserId = a.UserId,
                        Revision = a.Revision
                    };
                }

                return new TextOperation
                {
                    Type = "delete",
                    Position = bStart <= aStart ? bStart : aStart,
                    Length = newLen,
                    UserId = a.UserId,
                    Revision = a.Revision
                };
            }

            // ── NOOP ─────────────────────────────────────────────────────────
            return a;
        }

        /// <summary>
        /// Ограничивает позицию и длину операции реальными границами документа.
        /// </summary>
        private static TextOperation ClampOperation(TextOperation op, int docLength)
        {
            if (op.Type == "noop") return op;

            int pos = Math.Max(0, Math.Min(op.Position, docLength));

            if (op.Type == "insert")
                return new TextOperation { Type = "insert", Position = pos, Text = op.Text, UserId = op.UserId, Revision = op.Revision };

            if (op.Type == "delete")
            {
                int maxLen = docLength - pos;
                int len = Math.Max(0, Math.Min(op.Length, maxLen));
                if (len == 0)
                    return new TextOperation { Type = "noop", Position = 0, UserId = op.UserId, Revision = op.Revision };
                return new TextOperation { Type = "delete", Position = pos, Length = len, UserId = op.UserId, Revision = op.Revision };
            }

            return op;
        }

        private static void ApplyToStringBuilder(StringBuilder sb, TextOperation op)
        {
            if (op.Type == "insert" && !string.IsNullOrEmpty(op.Text))
            {
                if (op.Position <= sb.Length)
                    sb.Insert(op.Position, op.Text);
            }
            else if (op.Type == "delete" && op.Length > 0)
            {
                if (op.Position >= 0 && op.Position + op.Length <= sb.Length)
                    sb.Remove(op.Position, op.Length);
            }
            // noop — ничего не делаем
        }

        // ─────────────────────────────────────────────────────────────────────
        // Внутренние типы
        // ─────────────────────────────────────────────────────────────────────

        private class DocumentState
        {
            public readonly object Lock = new();
            public StringBuilder Content { get; set; } = new();
            public int Revision { get; set; }
            /// <summary>
            /// История применённых операций (уже трансформированных, на серверной ревизии).
            /// op.Revision здесь = ревизия документа ПОСЛЕ применения этой операции.
            /// </summary>
            public List<TextOperation> History { get; } = new();
        }

        internal record EditorEntry(string ConnectionId, string UserName);
    }
}