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
            // Если TemplateId = 0 или null, находим подходящий шаблон
            if (layout.TemplateId == 0)
            {
                // Пробуем найти существующий шаблон
                var availableTemplate = await _context.Templates
                    .OrderBy(t => t.TemplateId)
                    .FirstOrDefaultAsync();
        
                if (availableTemplate != null)
                {
                    layout.TemplateId = availableTemplate.TemplateId;
                }
                else
                {
                    // Если шаблонов вообще нет в базе
                    // Нужно либо создать шаблон по умолчанию, либо убрать ограничение
                    throw new InvalidOperationException(
                        "Не найден ни один шаблон. Создайте хотя бы один шаблон в системе.");
                }
            }
    
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

        // ДОБАВЛЕН: Метод удаления макета страницы
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
                .Include(pl => pl.LayoutElements)  // ДОБАВЬТЕ ЭТОТ БЛОК
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
        
        // Размещение части статьи на блоке макета с использованием CurrentYOffset
        public async Task<LayoutElement> PlaceArticlePartOnLayoutBlockAsync(LayoutBlockInfo blockInfo, int articleId, double height)
        {
            // Правильный расчет X позиции: columnIndex * (columnWidth + columnGap)
            double columnSpacing = blockInfo.ColumnWidth + blockInfo.ColumnGap;
            double xPosition = blockInfo.ColumnIndex * columnSpacing;
            
            // Используем CurrentYOffset для вычисления позиции
            var position = JsonSerializer.Serialize(new 
            { 
                X = xPosition,
                Y = blockInfo.CurrentYOffset 
            });
            
            var size = JsonSerializer.Serialize(new 
            { 
                Width = blockInfo.ColumnWidth, 
                Height = height 
            });
            
            var element = new LayoutElement
            {
                PageLayoutId = blockInfo.PageLayoutId,
                ArticleId = articleId,
                Type = ElementType.TextFrame,
                Position = position,
                Size = size,
                CreatedDate = DateTime.Now
            };

            _context.LayoutElements.Add(element);
            await _context.SaveChangesAsync();
            
            // Обновляем CurrentYOffset для следующего элемента
            blockInfo.CurrentYOffset += height + 0.5; // Добавляем высоту элемента и отступ
            
            return element;
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
            // ФИКС: Обработка TemplateId
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

        public async Task<PageLayout> ConfigurePageLayoutAsync(int pageLayoutId, PageLayoutConfiguration config)
        {
            var layout = await _context.PageLayouts.FindAsync(pageLayoutId);
            if (layout == null)
                throw new ArgumentException("Page layout not found");

            // Обновляем настройки макета
            layout.ColumnCount = config.ColumnCount;
            layout.MarginTop = config.MarginTop;
            layout.MarginBottom = config.MarginBottom;
            layout.MarginLeft = config.MarginLeft;
            layout.MarginRight = config.MarginRight;
            layout.ColumnGap = config.ColumnGap;
            layout.TextAreaWidth = config.TextAreaWidth;
            layout.TextAreaHeight = config.TextAreaHeight;
            layout.ImageAreaWidth = config.ImageAreaWidth;
            layout.ImageAreaHeight = config.ImageAreaHeight;
            
            // ФИКС: Обработка TemplateId
            if (config.TemplateId.HasValue && config.TemplateId.Value > 0)
            {
                layout.TemplateId = config.TemplateId.Value;
            }
            else
            {
                layout.TemplateId = 0;
            }

            layout.UpdatedAt = DateTime.UtcNow;

            // Сохраняем настройки в JSON
            var layoutSettings = new
            {
                config.ColumnCount,
                config.MarginTop,
                config.MarginBottom,
                config.MarginLeft,
                config.MarginRight,
                config.ColumnGap,
                config.TextAreaWidth,
                config.TextAreaHeight,
                config.ImageAreaWidth,
                config.ImageAreaHeight
            };
            
            layout.LayoutSettings = JsonSerializer.Serialize(layoutSettings);

            await _context.SaveChangesAsync();
            return layout;
        }

        public async Task<bool> ValidateLayoutConfigurationAsync(PageLayoutConfiguration config)
        {
            // Проверяем валидность конфигурации
            if (config.ColumnCount < 1 || config.ColumnCount > 4)
                return false;

            if (config.MarginTop < 0 || config.MarginBottom < 0 || 
                config.MarginLeft < 0 || config.MarginRight < 0)
                return false;

            if (config.TextAreaWidth <= 0 || config.TextAreaHeight <= 0)
                return false;

            return true;
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

            // Проверяем, помещаются ли элементы в новые границы
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
            // Улучшенная проверка из обеих версий
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
                // Комбинированный подход: увеличиваем размер и устанавливаем auto-height
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

        public async Task<List<Template>> GetTemplatesAsync()
        {
            return await _context.Templates.ToListAsync();
        }

        public async Task<List<Template>> GetAvailableTemplatesAsync()
        {
            return await _context.Templates.ToListAsync();
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