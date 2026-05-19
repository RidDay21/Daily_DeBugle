using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class PublicationServiceTests
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
    public async Task CreatePublicationAsync_ShouldThrow_WhenNameExists()
    {
        var dbContext = GetDbContext();
        var service = new PublicationService(dbContext);
        var pub = new Publication { Name = "Times", IsActive = true };
        dbContext.Publications.Add(pub);
        await dbContext.SaveChangesAsync();

        var newPub = new Publication { Name = "Times" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreatePublicationAsync(newPub));
    }

    [Fact]
    public async Task DeletePublicationAsync_ShouldSetIsActiveToFalse()
    {
        var dbContext = GetDbContext();
        var service = new PublicationService(dbContext);
        var pub = new Publication { Name = "To Delete", IsActive = true };
        dbContext.Publications.Add(pub);
        await dbContext.SaveChangesAsync();

        await service.DeletePublicationAsync(pub.PublicationId);

        var deletedPub = await dbContext.Publications.FindAsync(pub.PublicationId);
        Assert.False(deletedPub.IsActive);
    }
}
