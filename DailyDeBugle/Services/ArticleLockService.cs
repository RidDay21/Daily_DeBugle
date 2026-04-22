using DailyDeBugle.Data;
using DailyDeBugle.Hubs;
using DailyDeBugle.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services;

public class ArticleLockService : IArticleLockService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<CollaborationHub> _hubContext;

    public ArticleLockService(ApplicationDbContext context, IHubContext<CollaborationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<bool> TryLockArticleAsync(int articleId, int userId)
    {
        await CleanExpiredLocksAsync();

        var article = await _context.Articles
            .Include(a => a.LockedByUser)
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);

        if (article == null) return false;

        // Если статья уже заблокирована другим пользователем
        if (article.LockedByUserId.HasValue && article.LockedByUserId != userId)
            return false;

        var wasAlreadyLockedByThisUser = article.LockedByUserId == userId;

        article.LockedByUserId = userId;
        article.LockedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (!wasAlreadyLockedByThisUser)
        {
            var userName = article.LockedByUser?.Username ?? "Пользователь";
            await NotifyLockChanged(articleId, true, userName);
        }

        return true;
    }

    public async Task UnlockArticleAsync(int articleId, int userId)
    {
        try
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null || article.LockedByUserId != userId) return;

            article.LockedByUserId = null;
            article.LockedAt = null;
            await _context.SaveChangesAsync();

            await NotifyLockChanged(articleId, false, null);
        }
        catch (ObjectDisposedException)
        {
            // Игнорируем, так как контекст уже мёртв, а блокировку можно снять позже через фоновую очистку
        }
    }

    public async Task<(bool IsLocked, string? LockedByUserName)> GetLockStatusAsync(int articleId)
    {
        await CleanExpiredLocksAsync();

        var article = await _context.Articles
            .Include(a => a.LockedByUser)
            .FirstOrDefaultAsync(a => a.ArticleId == articleId);

        if (article?.LockedByUserId == null)
            return (false, null);

        return (true, article.LockedByUser?.Username);
    }

    public async Task ReleaseAllLocksForUserAsync(int userId)
    {
        var articles = await _context.Articles
            .Where(a => a.LockedByUserId == userId)
            .ToListAsync();

        foreach (var article in articles)
        {
            article.LockedByUserId = null;
            article.LockedAt = null;
        }

        if (articles.Any())
        {
            await _context.SaveChangesAsync();
            // Можно оповестить о снятии блокировок, но проще не заморачиваться
        }
    }

    private async Task CleanExpiredLocksAsync()
    {
        var expiration = DateTime.UtcNow.AddMinutes(-5);
        var expired = await _context.Articles
            .Where(a => a.LockedAt < expiration && a.LockedByUserId != null)
            .ToListAsync();

        foreach (var article in expired)
        {
            article.LockedByUserId = null;
            article.LockedAt = null;
        }

        if (expired.Any())
            await _context.SaveChangesAsync();
    }

    private async Task NotifyLockChanged(int articleId, bool isLocked, string? userName)
    {
        await _hubContext.Clients.Group($"article-{articleId}")
            .SendAsync("ArticleLockChanged", articleId, isLocked, userName);
    }
}