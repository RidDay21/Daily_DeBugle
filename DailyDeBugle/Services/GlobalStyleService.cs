using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
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
                    
                    // Проверяем, существует ли выпуск
                    var issue = await _context.Issues.FindAsync(issueId);
                    if (issue == null)
                        throw new ArgumentException($"Issue with ID {issueId} not found");
                    
                    styles = new GlobalTextStyle
                    {
                        IssueId = issueId,
                        PrimaryFont = "Times New Roman",
                        HeadingFont = "Times New Roman",
                        H1Size = 24,
                        H2Size = 20,
                        BodySize = 14,
                        BodyLineSpacing = 1.4,
                        HeadingLineSpacing = 1.2
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
                    // Обновляем существующие стили
                    _context.Entry(existing).CurrentValues.SetValues(styles);
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Добавляем новые стили
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
            try
            {
                var styles = await GetStylesForIssueAsync(issueId);
                var articles = await _context.Articles
                    .Where(a => a.IssueId == issueId)
                    .ToListAsync();

                foreach (var article in articles)
                {
                    // Применяем стили к каждой статье
                    article.FontFamily = styles.PrimaryFont;
                    article.FontSize = styles.BodySize;
                    article.LineSpacing = styles.BodyLineSpacing;
                    
                    _logger.LogInformation($"Applied styles to article {article.ArticleId}: " +
                                          $"Font={styles.PrimaryFont}, Size={styles.BodySize}pt");
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Applied styles to {articles.Count} articles in issue {issueId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error applying styles to issue {issueId}");
                throw;
            }
        }
    }
}