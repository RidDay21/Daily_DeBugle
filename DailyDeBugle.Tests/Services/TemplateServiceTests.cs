using DailyDeBugle.Data;
using DailyDeBugle.Models;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DailyDeBugle.Tests.Services;

public class TemplateServiceTests
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
    public async Task CreateTemplateAsync_ShouldSaveTemplate()
    {
        var dbContext = GetDbContext();
        var service = new TemplateService(dbContext);
        var template = new Template { Name = "Test Template" };

        var result = await service.CreateTemplateAsync(template);

        Assert.NotEqual(0, result.TemplateId);
        var saved = await dbContext.Templates.FindAsync(result.TemplateId);
        Assert.Equal("Test Template", saved.Name);
    }

    [Fact]
    public async Task GetPageLayoutCountForTemplateAsync_ShouldReturnCorrectCount()
    {
        var dbContext = GetDbContext();
        var service = new TemplateService(dbContext);
        var template = new Template { Name = "T" };
        dbContext.Templates.Add(template);
        await dbContext.SaveChangesAsync();

        dbContext.PageLayouts.Add(new PageLayout { TemplateId = template.TemplateId, IssueId = 1, PageNumber = 1 });
        await dbContext.SaveChangesAsync();

        var count = await service.GetPageLayoutCountForTemplateAsync(template.TemplateId);

        Assert.Equal(1, count);
    }
}
