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

        // === Базовые CRUD операции ===
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

        public async Task DeletePageLayoutAsync(int pageLayoutId)
        {
            var layout = await _context.PageLayouts
                .Include(pl => pl.LayoutElements)
                .FirstOrDefaultAsync(pl => pl.PageLayoutId == pageLayoutId);
            
            if (layout != null)
            {
                // Сначала удаляем все элементы
                _context.LayoutElements.RemoveRange(layout.LayoutElements);
                
                // Затем удаляем сам макет
                _context.PageLayouts.Remove(layout);
                
                await _context.SaveChangesAsync();
            }
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
                .Include(pl => pl.LayoutElements)
                .ThenInclude(le => le.AdvertisementBlock)
                .Where(pl => pl.IssueId == issueId)
                .OrderBy(pl => pl.PageNumber)
                .ToListAsync();
        }
        
        public async Task CreateDefaultTemplateIfNotExists()
        {
            if (!await _context.Templates.AnyAsync())
            {
                var defaultTemplate = new Template
                {
                    Name = "Стандартный (3 колонки)",
                    Description = "Стандартный макет с 3 колонками, подходит для большинства статей",
                    LayoutSettings = JsonSerializer.Serialize(new
                    {
                        ColumnCount = 3,
                        MarginTop = 2.5,
                        MarginBottom = 2.5,
                        MarginLeft = 2.0,
                        MarginRight = 2.0,
                        ColumnGap = 0.5
                    }),
                    DefaultColumnCount = 3,
                    DefaultMarginTop = 2.5,
                    DefaultMarginBottom = 2.5,
                    DefaultMarginLeft = 2.0,
                    DefaultMarginRight = 2.0,
                    DefaultColumnGap = 0.5,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
        
                _context.Templates.Add(defaultTemplate);
                await _context.SaveChangesAsync();
            }
        }

        // === Работа с элементами layout ===
        public async Task AddArticleToLayoutAsync(int pageLayoutId, int articleId, string position, string size)
        {
            var element = new LayoutElement
            {
                PageLayoutId = pageLayoutId,
                ArticleId = articleId,
                Type = ElementType.TextFrame,
                Position = position,
                Size = size,
                CreatedDate = DateTime.Now
            };

            _context.LayoutElements.Add(element);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveElementFromLayoutAsync(int layoutElementId)
        {
            var element = await _context.LayoutElements.FindAsync(layoutElementId);
            if (element != null)
            {
                _context.LayoutElements.Remove(element);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<LayoutElement> UpdateLayoutElementAsync(LayoutElement element)
        {
            _context.LayoutElements.Update(element);
            await _context.SaveChangesAsync();
            return element;
        }

        // === Расширенная функциональность ===
        public async Task<PageLayout> GetPageLayoutAsync(int issueId, int pageNumber)
        {
            return await _context.PageLayouts
                .Include(p => p.Template)
                .Include(p => p.LayoutElements)
                    .ThenInclude(le => le.Article)
                .Include(p => p.LayoutElements)
                    .ThenInclude(le => le.AdvertisementBlock)
                .FirstOrDefaultAsync(p => p.IssueId == issueId && p.PageNumber == pageNumber);
        }
        
        public async Task<PageLayout> CreatePageLayoutAsync(PageLayout layout)
        {
            if (layout.TemplateId == 0)
            {
                layout.TemplateId = 0;
            }
            
            _context.PageLayouts.Add(layout);
            await _context.SaveChangesAsync();
            return layout;
        }

        public async Task<List<PageLayout>> GetIssueLayoutsAsync(int issueId)
        {
            return await _context.PageLayouts
                .Include(p => p.Template)
                .Where(p => p.IssueId == issueId)
                .OrderBy(p => p.PageNumber)
                .ToListAsync();
        }

        public async Task<List<string>> CheckLayoutConflictsAsync(int pageLayoutId)
        {
            var conflicts = new List<string>();
            var layout = await _context.PageLayouts
                .Include(p => p.LayoutElements)
                .FirstOrDefaultAsync(p => p.PageLayoutId == pageLayoutId);

            if (layout == null)
            {
                conflicts.Add("Макет страницы не найден");
                return conflicts;
            }

            // Проверяем, помещаются ли элементы в границы
            foreach (var element in layout.LayoutElements)
            {
                try
                {
                    var position = JsonSerializer.Deserialize<LayoutPosition>(element.Position);
                    var size = JsonSerializer.Deserialize<LayoutSize>(element.Size);

                    if (position != null && size != null)
                    {
                        if (position.X + size.Width > layout.TextAreaWidth)
                        {
                            conflicts.Add($"Элемент выходит за границы по ширине (страница {layout.PageNumber})");
                        }

                        if (position.Y + size.Height > layout.TextAreaHeight)
                        {
                            conflicts.Add($"Элемент выходит за границы по высоте (страница {layout.PageNumber})");
                        }
                    }
                }
                catch (JsonException)
                {
                    conflicts.Add($"Ошибка в данных элемента на странице {layout.PageNumber}");
                }
            }

            return conflicts;
        }

        // === Общие методы ===
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

        public async Task<bool> CheckTextOverflowAsync(LayoutElement element)
        {
            if (element.ArticleId == null) return false;
            
            var article = await _context.Articles.FindAsync(element.ArticleId);
            if (article == null) return false;

            // Проверка через оценку строк
            var estimatedLines = article.Content.Length / 80;
            var availableHeight = 20.0; // базовое значение
            
            // Если есть данные о высоте текстовой области
            var layout = await _context.PageLayouts.FindAsync(element.PageLayoutId);
            if (layout != null && layout.TextAreaHeight > 0)
            {
                availableHeight = layout.TextAreaHeight;
            }
            
            return estimatedLines * 0.4 > availableHeight;
        }

        public async Task AdjustTextFlowAsync(LayoutElement element)
        {
            var hasOverflow = await CheckTextOverflowAsync(element);
            if (hasOverflow)
            {
                var size = JsonSerializer.Deserialize<LayoutSize>(element.Size);
                size.Width += 50;
                element.Size = JsonSerializer.Serialize(new { Width = size.Width, Height = "auto" });
                await UpdateLayoutElementAsync(element);
            }
        }

        public async Task UpdateIssueStatusAsync(int issueId, IssueStatus status)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue != null)
            {
                issue.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Article>> GetApprovedArticlesForIssueAsync(int issueId)
        {
            return await _context.Articles
                .Where(a => a.IssueId == issueId && a.Status == ArticleStatus.Approved)
                .ToListAsync();
        }
        
        // Добавим этот метод в LayoutService
        public async Task AddAdvertisementToLayoutAsync(int pageLayoutId, int advertisementId, string position, string size, string textFlow = "None")
        {
            var element = new LayoutElement
            {
                PageLayoutId = pageLayoutId,
                AdvertisementBlockId = advertisementId,
                Type = ElementType.AdBlock,
                Position = position,
                Size = size,
                TextFlow = textFlow,
                CreatedDate = DateTime.Now
            };

            _context.LayoutElements.Add(element);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Template>> GetTemplatesAsync()
        {
            return await _context.Templates.ToListAsync();
        }

        public async Task<List<Template>> GetAvailableTemplatesAsync()
        {
            return await _context.Templates.ToListAsync();
        }

        public async Task<LayoutElement?> GetLayoutElementAsync(int layoutElementId)
        {
            return await _context.LayoutElements
                .Include(le => le.AdvertisementBlock)
                .FirstOrDefaultAsync(le => le.LayoutElementId == layoutElementId);
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