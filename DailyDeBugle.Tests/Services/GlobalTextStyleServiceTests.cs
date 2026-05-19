using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class GlobalTextStyleServiceTests
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var dbContext = new ApplicationDbContext(options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    [Fact]
    public async Task GetStylesForIssueAsync_ShouldCreateDefault_WhenNoneExists()
    {
        var dbContext = GetDbContext();
        var service = new GlobalTextStyleService(dbContext, NullLogger<GlobalTextStyleService>.Instance);
        var issue = new Issue { IssueNumber = "1", PublicationId = 1, IssueDate = DateTime.UtcNow };
        dbContext.Issues.Add(issue);
        await dbContext.SaveChangesAsync();

        var styles = await service.GetStylesForIssueAsync(issue.IssueId);

        Assert.NotNull(styles);
        Assert.Equal("Times New Roman", styles.PrimaryFont);
        Assert.Equal(issue.IssueId, styles.IssueId);
    }

    [Fact]
    public async Task ApplyStylesToIssueAsync_ShouldUpdateArticles()
    {
        var dbContext = GetDbContext();
        var service = new GlobalTextStyleService(dbContext, NullLogger<GlobalTextStyleService>.Instance);
        var issue = new Issue { IssueNumber = "1", PublicationId = 1, IssueDate = DateTime.UtcNow };
        dbContext.Issues.Add(issue);
        await dbContext.SaveChangesAsync();

        var article = new Article { Headline = "H", Content = "C", AuthorId = 1, IssueId = issue.IssueId };
        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync();

        var styles = await service.GetStylesForIssueAsync(issue.IssueId);
        styles.PrimaryFont = "Arial";
        styles.BodySize = 12;
        await service.SaveStylesAsync(styles);

        await service.ApplyStylesToIssueAsync(issue.IssueId);

        var updatedArticle = await dbContext.Articles.FindAsync(article.ArticleId);
        Assert.Equal("Arial", updatedArticle.FontFamily);
        Assert.Equal(12, updatedArticle.FontSize);
    }
}
