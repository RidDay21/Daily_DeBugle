using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DailyDeBugle.Data;
using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ApplicationDbContext _context;

        public LayoutService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PageLayout> CreatePageLayoutAsync(PageLayout layout)
        {
            _context.PageLayouts.Add(layout);
            await _context.SaveChangesAsync();
            return layout;
        }

        public async Task<PageLayout> UpdatePageLayoutAsync(PageLayout layout)
        {
            layout.UpdatedAt = DateTime.UtcNow;
            _context.PageLayouts.Update(layout);
            await _context.SaveChangesAsync();
            return layout;
        }

        public async Task ApplyTemplateAsync(int pageLayoutId, int templateId)
        {
            var layout = await _context.PageLayouts.FindAsync(pageLayoutId);
            var template = await _context.Templates.FindAsync(templateId);

            if (layout != null && template != null)
            {
                layout.TemplateId = templateId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckTextOverflowAsync(LayoutElement element)
        {
            // Логика проверки переполнения текста
            var layout = await _context.PageLayouts
                .Include(p => p.LayoutElements)
                .FirstOrDefaultAsync(p => p.PageLayoutId == element.PageLayoutId);
                
            if (layout == null) return false;

            // Простая проверка - если текст слишком длинный для области
            var textContent = element.Article?.Content ?? string.Empty;
            var estimatedLines = textContent.Length / 80; // Примерная оценка
            var availableHeight = layout.TextAreaHeight;
            
            // Предполагаем, что каждая строка занимает ~0.4 см
            return estimatedLines * 0.4 > availableHeight;
        }

        public async Task AdjustTextFlowAsync(LayoutElement element)
        {
            // Логика автоматической регулировки текста
            var hasOverflow = await CheckTextOverflowAsync(element);
            if (hasOverflow)
            {
                // Уменьшаем размер шрифта или переносим часть текста
                // В реальной реализации здесь была бы сложная логика
                element.Size = JsonSerializer.Serialize(new { Width = element.Size, Height = "auto" });
                await _context.SaveChangesAsync();
            }
        }

        // Реализация методов для Use Case 9
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
            
            if (config.TemplateId.HasValue)
            {
                layout.TemplateId = config.TemplateId.Value;
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
                    var position = JsonSerializer.Deserialize<Position>(element.Position);
                    var size = JsonSerializer.Deserialize<Size>(element.Size);

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
        
        public async Task<List<Template>> GetAvailableTemplatesAsync()
        {
            return await _context.Templates.ToListAsync();
        }
    }

    // Вспомогательные классы для десериализации
    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Size
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }
}