using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DailyDeBugle.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ApplicationDbContext _context;

        public LayoutService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Этот метод заменяет отдельные Create и Update методы
        public async Task CreateOrUpdatePageLayoutAsync(PageLayout layout)
        {
            if (layout.PageLayoutId == 0)
            {
                _context.PageLayouts.Add(layout);
            }
            else
            {
                _context.PageLayouts.Update(layout);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<PageLayout> GetPageLayoutAsync(int pageLayoutId)
        {
            return await _context.PageLayouts
                .Include(pl => pl.LayoutElements)
                .ThenInclude(le => le.Article)
                .FirstOrDefaultAsync(pl => pl.PageLayoutId == pageLayoutId);
        }

        public async Task<List<PageLayout>> GetPageLayoutsForIssueAsync(int issueId)
        {
            return await _context.PageLayouts
                .Include(pl => pl.LayoutElements)
                .ThenInclude(le => le.Article)
                .Where(pl => pl.IssueId == issueId)
                .OrderBy(pl => pl.PageNumber)
                .ToListAsync();
        }

        // Новый метод для добавления статьи в layout
        // В LayoutService
        public async Task AddArticleToLayoutAsync(int pageLayoutId, int articleId, string position, string size)
        {
            var element = new LayoutElement
            {
                PageLayoutId = pageLayoutId,
                ArticleId = articleId,
                Type = ElementType.TextFrame, // Указываем тип элемента
                Position = position,
                Size = size,
                CreatedDate = DateTime.Now
            };

            _context.LayoutElements.Add(element);
            await _context.SaveChangesAsync();
        }

        // Новый метод для удаления элемента из layout
        public async Task RemoveElementFromLayoutAsync(int layoutElementId)
        {
            var element = await _context.LayoutElements.FindAsync(layoutElementId);
            if (element != null)
            {
                _context.LayoutElements.Remove(element);
                await _context.SaveChangesAsync();
            }
        }

        // Новый метод для обновления статуса выпуска
        public async Task UpdateIssueStatusAsync(int issueId, IssueStatus status)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue != null)
            {
                issue.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LayoutElement> UpdateLayoutElementAsync(LayoutElement element)
        {
            _context.LayoutElements.Update(element);
            await _context.SaveChangesAsync();
            return element;
        }

        public async Task ApplyTemplateAsync(int pageLayoutId, int templateId)
        {
            var pageLayout = await _context.PageLayouts.FindAsync(pageLayoutId);
            var template = await _context.Templates.FindAsync(templateId);
            
            if (pageLayout != null && template != null)
            {
                pageLayout.TemplateId = templateId;
                pageLayout.LayoutSettings = template.LayoutSettings;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Template>> GetTemplatesAsync()
        {
            return await _context.Templates
                .Where(t => t.IsActive)
                .ToListAsync();
        }

        public async Task<bool> CheckTextOverflowAsync(LayoutElement element)
        {
            if (element.ArticleId == null) return false;
            
            var article = await _context.Articles.FindAsync(element.ArticleId);
            if (article == null) return false;

            // Простая проверка переполнения (можно усложнить)
            var position = JsonSerializer.Deserialize<LayoutPosition>(element.Position);
            var size = JsonSerializer.Deserialize<LayoutSize>(element.Size);
            
            // Предположим, что в 100px помещается ~100 символов
            double availableSpace = (size.Width * size.Height) / 100;
            return article.Content.Length > availableSpace;
        }

        public async Task AdjustTextFlowAsync(LayoutElement element)
        {
            // Увеличиваем размер элемента при переполнении
            var size = JsonSerializer.Deserialize<LayoutSize>(element.Size);
            size.Width += 50;
            size.Height += 50;
            
            element.Size = JsonSerializer.Serialize(size);
            await UpdateLayoutElementAsync(element);
        }

        public async Task<List<Article>> GetApprovedArticlesForIssueAsync(int issueId)
        {
            return await _context.Articles
                .Where(a => a.IssueId == issueId && a.Status == ArticleStatus.Approved)
                .ToListAsync();
        }
    }

    // Вспомогательные классы для позиционирования
    public class LayoutPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class LayoutSize
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
}