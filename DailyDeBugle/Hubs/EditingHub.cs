using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using DailyDeBugle.Services;
using DailyDeBugle.Models;

namespace DailyDeBugle.Hubs
{
    public class EditingHub : Hub
    {
        private readonly CollaborativeEditingService _editingService;

        // connectionId -> список articleId, к которым подключён этот клиент
        private static readonly ConcurrentDictionary<string, HashSet<int>> _connectionArticles = new();

        public EditingHub(ICollaborativeEditingService editingService)
        {
            // Кастуем к конкретному типу, чтобы получить доступ к internal-методам.
            // Если ICollaborativeEditingService внедряется как Singleton CollaborativeEditingService,
            // это всегда безопасно. При желании можно расширить интерфейс.
            _editingService = (CollaborativeEditingService)editingService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Управление соединением
        // ─────────────────────────────────────────────────────────────────────

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Убираем редактора из всех документов, в которых он был
            var affectedArticles = _editingService.RemoveEditorFromAll(connectionId);

            // Удаляем запись о соединении
            _connectionArticles.TryRemove(connectionId, out _);

            // Оповещаем группы об изменении состава редакторов
            foreach (var articleId in affectedArticles)
            {
                await BroadcastEditorList(articleId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Методы, вызываемые клиентом
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Клиент входит в документ: подписывается на группу и получает актуальное содержимое.
        /// </summary>
        public async Task JoinDocument(int articleId, string userName)
        {
            var connectionId = Context.ConnectionId;

            await Groups.AddToGroupAsync(connectionId, $"doc-{articleId}");

            // Регистрируем редактора
            _editingService.AddEditor(articleId, connectionId, userName);

            // Запоминаем связь connectionId -> articleId
            var articles = _connectionArticles.GetOrAdd(connectionId, _ => new HashSet<int>());
            lock (articles) { articles.Add(articleId); }

            // Отправляем клиенту актуальный документ
            var (content, revision) = await _editingService.GetDocumentAsync(articleId);
            await Clients.Caller.SendAsync("DocumentLoaded", content, revision);

            // Оповещаем всю группу об изменении списка редакторов
            await BroadcastEditorList(articleId);
        }

        /// <summary>
        /// Клиент покидает документ (нормальный выход).
        /// </summary>
        public async Task LeaveDocument(int articleId)
        {
            var connectionId = Context.ConnectionId;

            await Groups.RemoveFromGroupAsync(connectionId, $"doc-{articleId}");

            _editingService.RemoveEditor(articleId, connectionId);

            if (_connectionArticles.TryGetValue(connectionId, out var articles))
            {
                lock (articles) { articles.Remove(articleId); }
            }

            await BroadcastEditorList(articleId);
        }

        /// <summary>
        /// Клиент отправляет операцию редактирования.
        /// </summary>
        public async Task SendOperation(int articleId, TextOperation operation)
        {
            await _editingService.ApplyOperationAsync(articleId, operation, Context.ConnectionId);
        }

        /// <summary>
        /// Первый вошедший редактор инициализирует содержимое документа на сервере.
        /// Если документ уже существует (другой редактор вошёл раньше), вызов игнорируется.
        /// </summary>
        public void InitDocument(int articleId, string content)
        {
            _editingService.SetDocument(articleId, content);
        }

        /// <summary>
        /// Получить текущий список редакторов (для запроса при инициализации).
        /// </summary>
        public async Task RequestEditorList(int articleId)
        {
            var names = _editingService.GetActiveEditorNames(articleId);
            await Clients.Caller.SendAsync("ActiveEditorsUpdate", articleId, names);
        }

        /// <summary>
        /// Страница дашборда подписывается, чтобы получать ActiveEditorsUpdate для всех статей.
        /// </summary>
        public async Task JoinDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "review-dashboard");
        }

        public async Task LeaveDashboard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "review-dashboard");
        }

        /// <summary>
        /// Редактор сообщает всем в группе, что статья финализирована.
        /// Сервер ретранслирует событие остальным редакторам и дашборду.
        /// </summary>
        public async Task ArticleFinalized(int articleId)
        {
            // Удаляем документ из памяти сервиса — история операций больше не нужна
            _editingService.RemoveDocument(articleId);

            await Clients.Group($"doc-{articleId}").SendAsync("ArticleFinalized", articleId);
            await Clients.Group("review-dashboard").SendAsync("ArticleFinalized", articleId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Вспомогательные методы
        // ─────────────────────────────────────────────────────────────────────

        private async Task BroadcastEditorList(int articleId)
        {
            var names = _editingService.GetActiveEditorNames(articleId);
            // Рассылаем в группу doc-{articleId} и дополнительно в review-dashboard для дашборда
            await Clients.Group($"doc-{articleId}").SendAsync("ActiveEditorsUpdate", articleId, names);
            await Clients.Group("review-dashboard").SendAsync("ActiveEditorsUpdate", articleId, names);
        }
    }
}