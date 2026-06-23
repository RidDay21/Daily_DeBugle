using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class AdvertisementServiceTests
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
    public async Task GetAvailableAdsAsync_ShouldReturnOnlyActiveAds()
    {
        var dbContext = GetDbContext();
        var service = new AdvertisementService(dbContext, NullLogger<AdvertisementService>.Instance);
        var today = DateTime.UtcNow.Date;

        dbContext.AdvertisementBlocks.AddRange(new List<AdvertisementBlock>
        {
            new AdvertisementBlock { Advertiser = "Active", StartDate = today.AddDays(-1), EndDate = today.AddDays(1), Content = "test" },
            new AdvertisementBlock { Advertiser = "Expired", StartDate = today.AddDays(-5), EndDate = today.AddDays(-1), Content = "test" },
            new AdvertisementBlock { Advertiser = "Future", StartDate = today.AddDays(1), EndDate = today.AddDays(5), Content = "test" }
        });
        await dbContext.SaveChangesAsync();

        var result = await service.GetAvailableAdsAsync();

        Assert.Single(result);
        Assert.Equal("Active", result[0].Advertiser);
    }

    [Fact]
    public async Task SearchAdsAsync_ShouldFilterByAdvertiserName()
    {
        var dbContext = GetDbContext();
        var service = new AdvertisementService(dbContext, NullLogger<AdvertisementService>.Instance);
        var today = DateTime.UtcNow.Date;

        dbContext.AdvertisementBlocks.AddRange(new List<AdvertisementBlock>
        {
            new AdvertisementBlock { Advertiser = "Coca Cola", StartDate = today, EndDate = today, Content = "drink" },
            new AdvertisementBlock { Advertiser = "Pepsi", StartDate = today, EndDate = today, Content = "drink" }
        });
        await dbContext.SaveChangesAsync();

        var result = await service.SearchAdsAsync("Coca");

        Assert.Single(result);
        Assert.Equal("Coca Cola", result[0].Advertiser);
    }
}
