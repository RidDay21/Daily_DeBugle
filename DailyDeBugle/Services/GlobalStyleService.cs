using DailyDeBugle.Data;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services;

public class GlobalTextStyleService : IGlobalTextStyleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<GlobalTextStyleService> _logger;

    public GlobalTextStyleService(ApplicationDbContext context, ILogger<GlobalTextStyleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GlobalTextStyle> GetStylesForIssueAsync(int issueId)
    {
        try
        {
            var styles = await _context.GlobalTextStyles
                .Include(g => g.Issue)
                .FirstOrDefaultAsync(g => g.IssueId == issueId);

            if (styles == null)
            {
                _logger.LogInformation($"Creating default GlobalTextStyle for issue {issueId}");
                
                styles = new GlobalTextStyle
                {
                    IssueId = issueId,
                    PrimaryFont = "Times New Roman",
                    HeadingFont = "Times New Roman",
                    H1Size = 24,
                    H2Size = 20,
                    BodySize = 14,
                    BodyLineSpacing = 1.4,
                    HeadingLineSpacing = 1.2,
                    ColumnCount = 2,
                    ColumnGap = 1.0
                };
                
                _context.GlobalTextStyles.Add(styles);
                await _context.SaveChangesAsync();
                
                // Reload with included Issue
                styles = await _context.GlobalTextStyles
                    .Include(g => g.Issue)
                    .FirstOrDefaultAsync(g => g.IssueId == issueId);
            }

            return styles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting GlobalTextStyle for issue {issueId}");
            throw;
        }
    }

    public async Task<GlobalTextStyle> SaveStylesAsync(GlobalTextStyle styles)
    {
        try
        {
            var existing = await _context.GlobalTextStyles
                .FirstOrDefaultAsync(g => g.Id == styles.Id);

            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(styles);
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                styles.CreatedAt = DateTime.UtcNow;
                styles.UpdatedAt = DateTime.UtcNow;
                _context.GlobalTextStyles.Add(styles);
            }

            await _context.SaveChangesAsync();
            return styles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving GlobalTextStyle for issue {styles.IssueId}");
            throw;
        }
    }

    public async Task ApplyStylesToIssueAsync(int issueId)
    {
        // Apply styles to all articles in the issue
        var styles = await GetStylesForIssueAsync(issueId);
        var articles = await _context.Articles
            .Where(a => a.IssueId == issueId)
            .ToListAsync();

        foreach (var article in articles)
        {
            // Apply styles to article content
            _logger.LogInformation($"Applying styles to article {article.ArticleId}");
        }

        await _context.SaveChangesAsync();
    }
}