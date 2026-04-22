namespace DailyDeBugle.Services;

public interface IArticleLockService
{
    Task<bool> TryLockArticleAsync(int articleId, int userId);
    Task UnlockArticleAsync(int articleId, int userId);
    Task<(bool IsLocked, string? LockedByUserName)> GetLockStatusAsync(int articleId);
    Task ReleaseAllLocksForUserAsync(int userId);
}