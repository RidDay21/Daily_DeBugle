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
    public async Task CreateArticleAsync_ShouldSetInitialStatusAndDates()
    {
        var dbContext = GetDbContext();
        var service = new ArticleService(dbContext);
        var article = new Article
        {
            Headline = "Test Headline",
            Content = "Test Content",
            AuthorId = 1,
            IssueId = 1
        };
        var result = await service.CreateArticleAsync(article);
        Assert.Equal(ArticleStatus.Draft, result.Status);
        Assert.True((DateTime.UtcNow - result.CreatedDate).TotalSeconds < 5);
        var savedArticle = await dbContext.Articles.FindAsync(result.ArticleId);
        Assert.NotNull(savedArticle);
    }

    [Fact]
    public async Task DeleteArticleAsync_ShouldRemoveArticleAndRelatedData()
    {
        var dbContext = GetDbContext();
        var service = new ArticleService(dbContext);
        var article = new Article { Headline = "Delete Me", AuthorId = 1, IssueId = 1 };
        dbContext.Articles.Add(article);
        await dbContext.SaveChangesAsync();
        var comment = new Comment { ArticleId = article.ArticleId, Content = "Test" };
        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();
        var result = await service.DeleteArticleAsync(article.ArticleId);
        Assert.True(result);
        Assert.Null(await dbContext.Articles.FindAsync(article.ArticleId));
    }
}
