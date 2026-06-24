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
            _editingService = (CollaborativeEditingService)editingService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Управление соединением
        // ─────────────────────────────────────────────────────────────────────

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Если редактор отключился во время голосования, отменяем голосование для этой статьи
            if (_connectionArticles.TryGetValue(connectionId, out var articlesCopy))
            {
                lock (articlesCopy)
                {
                    foreach (var articleId in articlesCopy)
                    {
                        // Оповещаем группу, что голосование отменяется, так как один из участников отвалился
                        Clients.Group($"doc-{articleId}").SendAsync("ActionConfirmationCancelled", articleId);
                    }
                }
            }

            var affectedArticles = _editingService.RemoveEditorFromAll(connectionId);
            _connectionArticles.TryRemove(connectionId, out _);

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
        /// Запрос на подтверждение действия (Approve/Reject/Revision) от инициатора.
        /// </summary>
        public async Task RequestActionConfirmation(int articleId, string actionType, string initiatorName)
        {
            var names = _editingService.GetActiveEditorNames(articleId);

            // Если в документе сидит только 1 человек (сам инициатор), голосование не требуется.
            // Сразу возвращаем "approved = true" вызывающему клиенту (Caller).
            if (names.Count() <= 1)
            {
                await Clients.Caller.SendAsync("ActionConfirmationResult", articleId, true, "");
                return;
            }

            // Если редакторы есть, рассылаем всем ОСТАЛЬНЫМ в группе запрос на подтверждение
            await Clients.OthersInGroup($"doc-{articleId}")
                .SendAsync("ActionConfirmationRequested", articleId, actionType, initiatorName);
        }

        /// <summary>
        /// Ответ от других редакторов (approved = true/false).
        /// </summary>
        public async Task RespondToActionConfirmation(int articleId, bool approved)
        {
            if (approved)
            {
                // На Blazor-клиенте заложено коллективное принятие решений.
                // В данной реализации, если этот редактор согласен, мы пересылаем голос инициатору.
                // Примечание: Для полноценного консенсуса (если редакторов > 2) обычно собирается счетчик на сервере,
                // но в рамках текущей логики фронтенда отправляем результат в группу/инициатору.
                await Clients.Group($"doc-{articleId}").SendAsync("ActionConfirmationResult", articleId, true, "");
            }
            else
            {
                // Если хотя бы ОДИН редактор нажал "Нет, отменить", 
                // то мы сразу шлем отмену (approved = false) всем участникам.
                await Clients.Group($"doc-{articleId}")
                    .SendAsync("ActionConfirmationResult", articleId, false, "Действие отклонено одним из редакторов.");
            }
        }

        public async Task JoinDocument(int articleId, string userName)
        {
            var connectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, $"doc-{articleId}");

            _editingService.AddEditor(articleId, connectionId, userName);

            var articles = _connectionArticles.GetOrAdd(connectionId, _ => new HashSet<int>());
            lock (articles) { articles.Add(articleId); }

            var (content, revision) = await _editingService.GetDocumentAsync(articleId);
            await Clients.Caller.SendAsync("DocumentLoaded", content, revision);

            await BroadcastEditorList(articleId);
        }

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

        public async Task SendOperation(int articleId, TextOperation operation)
        {
            await _editingService.ApplyOperationAsync(articleId, operation, Context.ConnectionId);
        }

        public void InitDocument(int articleId, string content)
        {
            _editingService.SetDocument(articleId, content);
        }

        public async Task RequestEditorList(int articleId)
        {
            var names = _editingService.GetActiveEditorNames(articleId);
            await Clients.Caller.SendAsync("ActiveEditorsUpdate", articleId, names);
        }

        public async Task JoinDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "review-dashboard");
        }

        public async Task LeaveDashboard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "review-dashboard");
        }

        public async Task ArticleFinalized(int articleId)
        {
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
            await Clients.Group($"doc-{articleId}").SendAsync("ActiveEditorsUpdate", articleId, names);
            await Clients.Group("review-dashboard").SendAsync("ActiveEditorsUpdate", articleId, names);
        }
    }
}