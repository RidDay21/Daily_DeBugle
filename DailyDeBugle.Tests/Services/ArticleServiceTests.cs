using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class ArticleServiceTests
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
    public async Task CalculateArticleStatisticsAsync_ShouldCalculateCorrectly()
    {
        var dbContext = GetDbContext();
        var service = new ArticleService(dbContext);
        var article = new Article 
        {
            Headline = "Test Headline",
            Content = "One two three. Four five six.",
            AuthorId = 1,
            IssueId = 1
        };
        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync();

        await service.CalculateArticleStatisticsAsync(article.ArticleId);

        var result = await dbContext.Articles.FindAsync(article.ArticleId);
        Assert.Equal(6, result.WordCount);
    }

    [Fact]
    public async Task SplitArticleForPaginationAsync_ShouldSplitText()
    {
        var dbContext = GetDbContext();
        var service = new ArticleService(dbContext);
        var article = new Article 
        {
            Headline = "Test Headline",
            Content = "This is a long text that should be split into multiple parts for testing.",
            AuthorId = 1,
            IssueId = 1
        };
        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync();

        var parts = await service.SplitArticleForPaginationAsync(article.ArticleId, 30);

        Assert.True(parts.Count > 1);
        Assert.Equal(1, parts[0].PartNumber);
        Assert.True(parts[0].IsBeginning);
    }
}
