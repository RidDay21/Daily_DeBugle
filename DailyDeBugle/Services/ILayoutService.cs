using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface ILayoutService
    {
        Task<PageLayout> CreatePageLayoutAsync(PageLayout layout);
        Task<PageLayout> UpdatePageLayoutAsync(PageLayout layout);
        Task ApplyTemplateAsync(int pageLayoutId, int templateId);
        Task<bool> CheckTextOverflowAsync(LayoutElement element);
        Task AdjustTextFlowAsync(LayoutElement element);
        
        Task<PageLayout> GetPageLayoutAsync(int issueId, int pageNumber);
        Task<List<PageLayout>> GetIssueLayoutsAsync(int issueId);
        Task<PageLayout> ConfigurePageLayoutAsync(int pageLayoutId, PageLayoutConfiguration config);
        Task<bool> ValidateLayoutConfigurationAsync(PageLayoutConfiguration config);
        Task<List<string>> CheckLayoutConflictsAsync(int pageLayoutId);
        Task<List<Template>> GetAvailableTemplatesAsync();
    }

    // Класс для конфигурации макета страницы
    public class PageLayoutConfiguration
    {
        public int PageNumber { get; set; }
        public int ColumnCount { get; set; } = 1;
        public double MarginTop { get; set; } = 1.0;
        public double MarginBottom { get; set; } = 1.0;
        public double MarginLeft { get; set; } = 1.0;
        public double MarginRight { get; set; } = 1.0;
        public double ColumnGap { get; set; } = 0.5;
        public double TextAreaWidth { get; set; } = 8.0;
        public double TextAreaHeight { get; set; } = 10.0;
        public double ImageAreaWidth { get; set; } = 8.0;
        public double ImageAreaHeight { get; set; } = 6.0;
        public int? TemplateId { get; set; }
    }
}