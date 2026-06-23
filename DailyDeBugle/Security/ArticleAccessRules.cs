using System.Security.Claims;
using DailyDeBugle.Models;

namespace DailyDeBugle.Security;

public static class ArticleAccessRules
{
    public const string UserIdClaimType = "UserId";

    public static bool IsAdmin(ClaimsPrincipal user) =>
        user.IsInRole(Roles.Admin);

    public static int? GetUserId(ClaimsPrincipal user)
    {
        var v = user.FindFirst(UserIdClaimType)?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }

    /// <summary>
    /// Текст и метаданные статьи может менять только автор (или Admin).
    /// </summary>
    public static bool CanEditArticleContent(ClaimsPrincipal user, Article article)
    {
        if (user.Identity?.IsAuthenticated != true)
            return false;
        if (IsAdmin(user))
            return true;
        var uid = GetUserId(user);
        return uid.HasValue && article.AuthorId == uid;
    }
}
