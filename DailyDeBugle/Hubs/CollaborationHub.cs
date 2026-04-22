using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DailyDeBugle.Hubs;

public class CollaborationHub : Hub
{
    public async Task JoinArticleGroup(int articleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"article-{articleId}");
    }

    public async Task LeaveArticleGroup(int articleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"article-{articleId}");
    }
}