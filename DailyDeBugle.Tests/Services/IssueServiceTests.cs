using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class IssueServiceTests
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
    public async Task CreateAsync_ShouldThrowException_WhenIssueNumberExists()
    {
        var dbContext = GetDbContext();
        var service = new IssueService(dbContext);
        var issue = new Issue { IssueNumber = "1", PublicationId = 1, IssueDate = DateTime.UtcNow };
        dbContext.Issues.Add(issue);
        await dbContext.SaveChangesAsync();
        var newIssue = new Issue { IssueNumber = "1", PublicationId = 1, IssueDate = DateTime.UtcNow };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(newIssue));
    }

    [Fact]
    public async Task PublishAsync_ShouldUpdateStatusToPublished()
    {
        var dbContext = GetDbContext();
        var service = new IssueService(dbContext);
        var issue = new Issue { IssueNumber = "2", PublicationId = 1, Status = IssueStatus.InProgress, IssueDate = DateTime.UtcNow };
        dbContext.Issues.Add(issue);
        await dbContext.SaveChangesAsync();
        await service.PublishAsync(issue.IssueId);
        var updatedIssue = await dbContext.Issues.FindAsync(issue.IssueId);
        Assert.Equal(IssueStatus.Published, updatedIssue.Status);
    }
}
