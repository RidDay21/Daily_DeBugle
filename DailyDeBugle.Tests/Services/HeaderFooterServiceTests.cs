using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class HeaderFooterServiceTests
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
    public async Task GetSettingsForIssueAsync_ShouldCreateDefault_WhenNoneExists()
    {
        var dbContext = GetDbContext();
        var service = new HeaderFooterService(dbContext, null!, null!);

        var settings = await service.GetSettingsForIssueAsync(1);

        Assert.NotNull(settings);
        Assert.Equal("{PublicationName}", settings.HeaderLeft);
        Assert.Equal(1, settings.IssueId);
    }

    [Fact]
    public async Task SaveSettingsAsync_ShouldUpdateExistingSettings()
    {
        var dbContext = GetDbContext();
        var service = new HeaderFooterService(dbContext, null!, null!);
        var settings = new HeaderFooterSettings 
        {
            IssueId = 1, 
            HeaderLeft = "Old",
            HeaderCenter = "",
            HeaderRight = "",
            FooterLeft = "",
            FooterCenter = "",
            FooterRight = ""
        };
        dbContext.HeaderFooterSettings.Add(settings);
        await dbContext.SaveChangesAsync();

        settings.HeaderLeft = "New";
        await service.SaveSettingsAsync(settings);

        var updated = await dbContext.HeaderFooterSettings.FindAsync(settings.Id);
        Assert.Equal("New", updated.HeaderLeft);
    }
}
