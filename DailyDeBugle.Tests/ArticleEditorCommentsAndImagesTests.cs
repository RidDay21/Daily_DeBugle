using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests;

public class ArticleEditorCommentsAndImagesTests
{
    private static ApplicationDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(int articleId, ArticleService svc)> SeedArticleAsync(ApplicationDbContext db)
    {
        var author = new User { Username = "a", Email = "a@x", PasswordHash = "x", Role = UserRole.Author };
        db.Users.Add(author);
        var pub = new Publication { Name = "P", Description = "d", Frequency = PublicationFrequency.Weekly, IsActive = true, CreatedDate = DateTime.UtcNow };
        db.Publications.Add(pub);
        await db.SaveChangesAsync();

        var issue = new Issue
        {
            IssueNumber = "1",
            IssueDate = DateTime.UtcNow,
            PublicationId = pub.PublicationId,
            Status = IssueStatus.InProgress
        };
        db.Issues.Add(issue);
        await db.SaveChangesAsync();

        var article = new Article
        {
            Headline = "T",
            Content = new string('w', 55),
            AuthorId = author.UserId,
            IssueId = issue.IssueId,
            Status = ArticleStatus.UnderReview
        };
        db.Articles.Add(article);
        await db.SaveChangesAsync();

        return (article.ArticleId, new ArticleService(db));
    }

    [Fact]
    public async Task AddEditorCommentAsync_persists_editor_flag()
    {
        await using var db = NewDb();
        var (id, svc) = await SeedArticleAsync(db);

        var c = await svc.AddEditorCommentAsync(id, "Please fix intro.");

        Assert.True(c.CommentId > 0);
        Assert.True(c.IsEditorComment);
        var fromDb = await db.Comments.FindAsync(c.CommentId);
        Assert.NotNull(fromDb);
        Assert.Equal(id, fromDb!.ArticleId);
    }

    [Fact]
    public async Task SendForRevisionAsync_stores_comment()
    {
        await using var db = NewDb();
        var (id, svc) = await SeedArticleAsync(db);

        await svc.SendForRevisionAsync(id, "Needs sources.");

        var comments = await db.Comments.Where(x => x.ArticleId == id).ToListAsync();
        Assert.Contains(comments, x => x.Content == "Needs sources." && x.IsEditorComment);
    }

    [Fact]
    public async Task AddImageAsync_links_to_article()
    {
        await using var db = NewDb();
        var (id, svc) = await SeedArticleAsync(db);

        var img = await svc.AddImageAsync(id, "/uploads/articles/1/x.png");

        Assert.True(img.ArticleImageId > 0);
        var list = await svc.GetImagesAsync(id);
        Assert.Single(list);
        Assert.Equal("/uploads/articles/1/x.png", list[0].ImagePath);
    }
}

