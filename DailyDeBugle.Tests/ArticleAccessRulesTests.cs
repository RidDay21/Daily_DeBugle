using System.Security.Claims;
using DailyDeBugle.Models;
using DailyDeBugle.Security;
using Xunit;

namespace DailyDeBugle.Tests;

public class ArticleAccessRulesTests
{
    [Fact]
    public void Author_can_edit_own_article()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "alice"),
                new Claim(ArticleAccessRules.UserIdClaimType, "7")
            },
            authenticationType: "test"));

        var article = new Article { AuthorId = 7, Headline = "H", Content = new string('x', 60) };

        Assert.True(ArticleAccessRules.CanEditArticleContent(user, article));
    }

    [Fact]
    public void Author_cannot_edit_someone_elses_article()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ArticleAccessRules.UserIdClaimType, "1") },
            "test"));

        var article = new Article { AuthorId = 2, Headline = "H", Content = new string('x', 60) };

        Assert.False(ArticleAccessRules.CanEditArticleContent(user, article));
    }

    [Fact]
    public void Admin_can_edit_any_article()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Role, Roles.Admin),
                new Claim(ArticleAccessRules.UserIdClaimType, "99")
            },
            "test"));

        var article = new Article { AuthorId = 2, Headline = "H", Content = new string('x', 60) };

        Assert.True(ArticleAccessRules.CanEditArticleContent(user, article));
    }
}

