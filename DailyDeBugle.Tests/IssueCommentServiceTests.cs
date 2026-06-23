using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests;

public class IssueCommentServiceTests
{
    private static ApplicationDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(int issueId, int readerId, IssueCommentService svc)> SeedPublishedIssueAsync(
        ApplicationDbContext db,
        IssueStatus status = IssueStatus.Published)
    {
        var reader = new User
        {
            Username = "reader1",
            Email = "r@x",
            PasswordHash = "x",
            Role = UserRole.Reader
        };
        db.Users.Add(reader);

        var pub = new Publication
        {
            Name = "P",
            Description = "d",
            Frequency = PublicationFrequency.Weekly,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        db.Publications.Add(pub);
        await db.SaveChangesAsync();

        var issue = new Issue
        {
            IssueNumber = "1",
            IssueDate = DateTime.UtcNow,
            PublicationId = pub.PublicationId,
            Status = status
        };
        db.Issues.Add(issue);
        await db.SaveChangesAsync();

        return (issue.IssueId, reader.UserId, new IssueCommentService(db));
    }

    [Fact]
    public async Task AddReaderCommentAsync_persists_for_published_issue()
    {
        await using var db = NewDb();
        var (issueId, readerId, svc) = await SeedPublishedIssueAsync(db);

        var comment = await svc.AddReaderCommentAsync(issueId, readerId, "  Great issue!  ");

        Assert.True(comment.IssueCommentId > 0);
        Assert.Equal("Great issue!", comment.Content);
        Assert.Equal(readerId, comment.UserId);

        var fromDb = await db.IssueComments.SingleAsync();
        Assert.Equal(issueId, fromDb.IssueId);
    }

    [Fact]
    public async Task AddReaderCommentAsync_rejects_non_published_issue()
    {
        await using var db = NewDb();
        var (issueId, readerId, svc) = await SeedPublishedIssueAsync(db, IssueStatus.InProgress);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.AddReaderCommentAsync(issueId, readerId, "Too early"));
    }

    [Fact]
    public async Task AddReaderCommentAsync_rejects_non_reader_role()
    {
        await using var db = NewDb();
        var (issueId, _, svc) = await SeedPublishedIssueAsync(db);

        var author = new User
        {
            Username = "author1",
            Email = "a@x",
            PasswordHash = "x",
            Role = UserRole.Author
        };
        db.Users.Add(author);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.AddReaderCommentAsync(issueId, author.UserId, "I am an author"));
    }

    [Fact]
    public async Task GetCommentsForIssueAsync_returns_chronological_order()
    {
        await using var db = NewDb();
        var (issueId, readerId, svc) = await SeedPublishedIssueAsync(db);

        await svc.AddReaderCommentAsync(issueId, readerId, "First");
        await Task.Delay(5);
        await svc.AddReaderCommentAsync(issueId, readerId, "Second");

        var comments = await svc.GetCommentsForIssueAsync(issueId);

        Assert.Equal(2, comments.Count);
        Assert.Equal("First", comments[0].Content);
        Assert.Equal("Second", comments[1].Content);
        Assert.Equal("reader1", comments[0].User.Username);
    }
}
