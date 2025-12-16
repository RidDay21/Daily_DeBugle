using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{

    public class IssueService : IIssueService
    {
        private readonly ApplicationDbContext _context;

        public IssueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Issue>> GetAllAsync()
        {
            return await _context.Issues
                .Include(i => i.Publication)
                .Include(i => i.Articles)
                .Include(i => i.PageLayouts)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<List<Issue>> GetIssuesAsync(int publicationId)
        {
            return await _context.Issues
                .Include(i => i.Publication)
                .Include(i => i.Articles)
                .Include(i => i.PageLayouts)
                .Where(i => i.PublicationId == publicationId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<Issue?> GetIssueAsync(int id)
        {
            return await _context.Issues
                .Include(i => i.Publication)
                .Include(i => i.Articles)
                    .ThenInclude(a => a.Author)
                .Include(i => i.PageLayouts)
                    .ThenInclude(p => p.LayoutElements)
                .Include(i => i.PageLayouts)
                    .ThenInclude(p => p.Template)
                .FirstOrDefaultAsync(i => i.IssueId == id);
        }

        public async Task<Issue> CreateAsync(Issue issue)
        {
            if (issue.IssueDate.Kind == DateTimeKind.Unspecified)
            {
                issue.IssueDate = DateTime.SpecifyKind(issue.IssueDate, DateTimeKind.Local).ToUniversalTime();
            }
            else
            {
                issue.IssueDate = issue.IssueDate.ToUniversalTime();
            }
            
            issue.Status = IssueStatus.InProgress;
            issue.IsFeatured = false;
            
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            
            return issue;
        }

        public async Task<Issue> UpdateAsync(Issue issue)
        {
            if (issue.IssueDate.Kind == DateTimeKind.Unspecified)
                issue.IssueDate = DateTime.SpecifyKind(issue.IssueDate, DateTimeKind.Local).ToUniversalTime();
            else
                issue.IssueDate = issue.IssueDate.ToUniversalTime();

            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();
            
            return issue;
        }

        public async Task DeleteAsync(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.Articles)
                .Include(i => i.PageLayouts)
                .FirstOrDefaultAsync(i => i.IssueId == id);
                
            if (issue != null)
            {
                // Удаляем связанные статьи
                _context.Articles.RemoveRange(issue.Articles);
                
                // Удаляем связанные макеты страниц
                _context.PageLayouts.RemoveRange(issue.PageLayouts);
                
                // Удаляем сам выпуск
                _context.Issues.Remove(issue);
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PublishAsync(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return false;

            issue.Status = IssueStatus.Published;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Publication>> GetPublicationsAsync()
        {
            return await _context.Publications
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        
        public async Task<Issue> GetFeaturedIssueAsync()
        {
            return await _context.Issues
                .Include(i => i.Articles.Where(a => a.Status == ArticleStatus.Approved))
                    .ThenInclude(a => a.Author)
                .Include(i => i.PageLayouts)
                    .ThenInclude(p => p.LayoutElements)
                .Include(i => i.PageLayouts)
                    .ThenInclude(p => p.Template)
                .Where(i => i.Status == IssueStatus.Published && i.IsFeatured)
                .OrderByDescending(i => i.IssueDate)
                .FirstOrDefaultAsync();
        }
    
        public async Task<List<Issue>> GetRecentIssuesAsync(int count)
        {
            return await _context.Issues
                .Include(i => i.Articles.Where(a => a.Status == ArticleStatus.Approved))
                .Include(i => i.PageLayouts)
                .Where(i => i.Status == IssueStatus.Published)
                .OrderByDescending(i => i.IssueDate)
                .Take(count)
                .ToListAsync();
        }
    
        public async Task<int> GetTotalIssuesCountAsync()
        {
            return await _context.Issues.CountAsync();
        }
    
        public async Task<int> GetPublishedIssuesCountAsync()
        {
            return await _context.Issues.CountAsync(i => i.Status == IssueStatus.Published);
        }
        
        public async Task<bool> SetAsFeaturedIssueAsync(int issueId)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null || issue.Status != IssueStatus.Published)
                return false;
        
            // Снимаем выделение со всех выпусков
            await _context.Issues
                .Where(i => i.IsFeatured)
                .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.IsFeatured, false));
        
            // Выделяем выбранный выпуск
            issue.IsFeatured = true;
            
            await _context.SaveChangesAsync();
        
            return true;
        }
        
        public async Task<bool> RemoveFromFeaturedAsync(int issueId)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null) return false;
    
            issue.IsFeatured = false;
            
            await _context.SaveChangesAsync();
    
            return true;
        }
        
        // Новый метод: Применить шаблон ко всему выпуску
        public async Task<bool> ApplyTemplateToIssueAsync(int issueId, int templateId)
        {
            try
            {
                // Проверяем существование выпуска и шаблона
                var issue = await _context.Issues.FindAsync(issueId);
                var template = await _context.Templates.FindAsync(templateId);
                
                if (issue == null || template == null)
                    return false;
                
                // Получаем все страницы выпуска
                var pageLayouts = await _context.PageLayouts
                    .Where(p => p.IssueId == issueId)
                    .ToListAsync();
                
                // Константы для размеров страницы (А4)
                const double PAGE_WIDTH = 21.0;
                const double PAGE_HEIGHT = 29.7;
                
                foreach (var pageLayout in pageLayouts)
                {
                    // Применяем настройки шаблона
                    pageLayout.TemplateId = templateId;
                    pageLayout.ColumnCount = template.DefaultColumnCount;
                    pageLayout.MarginTop = template.DefaultMarginTop;
                    pageLayout.MarginBottom = template.DefaultMarginBottom;
                    pageLayout.MarginLeft = template.DefaultMarginLeft;
                    pageLayout.MarginRight = template.DefaultMarginRight;
                    pageLayout.ColumnGap = template.DefaultColumnGap;
                    
                    // Пересчитываем текстовую область
                    pageLayout.TextAreaWidth = PAGE_WIDTH - pageLayout.MarginLeft - pageLayout.MarginRight;
                    pageLayout.TextAreaHeight = PAGE_HEIGHT - pageLayout.MarginTop - pageLayout.MarginBottom;
                    
                    // Округляем значения
                    pageLayout.TextAreaWidth = Math.Round(pageLayout.TextAreaWidth, 1);
                    pageLayout.TextAreaHeight = Math.Round(pageLayout.TextAreaHeight, 1);
                    pageLayout.ColumnGap = Math.Round(pageLayout.ColumnGap, 1);
                    
                    pageLayout.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при применении шаблона к выпуску: {ex.Message}");
                return false;
            }
        }
    }
}