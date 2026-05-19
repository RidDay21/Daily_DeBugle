using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class LayoutServiceTests
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
    public async Task CreateDefaultTemplateIfNotExists_ShouldCreateTemplate()
    {
        var dbContext = GetDbContext();
        var service = new LayoutService(dbContext);
        await service.CreateDefaultTemplateIfNotExists();
        var template = await dbContext.Templates.FirstOrDefaultAsync();
        Assert.NotNull(template);
    }
}
