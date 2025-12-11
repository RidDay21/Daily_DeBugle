using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface ILayoutService
    {
        Task<PageLayout> CreatePageLayoutAsync(PageLayout layout);
        Task CreateOrUpdatePageLayoutAsync(PageLayout layout);
        Task<PageLayout> GetPageLayoutAsync(int pageLayoutId);
        Task<List<PageLayout>> GetPageLayoutsForIssueAsync(int issueId);
        Task AddArticleToLayoutAsync(int pageLayoutId, int articleId, string position, string size);
        Task RemoveElementFromLayoutAsync(int layoutElementId);
        Task UpdateIssueStatusAsync(int issueId, IssueStatus status);
        Task DeletePageLayoutAsync(int pageLayoutId);
        Task<LayoutElement> UpdateLayoutElementAsync(LayoutElement element);
        Task ApplyTemplateAsync(int pageLayoutId, int templateId);
        Task<List<Template>> GetTemplatesAsync();
        Task<bool> CheckTextOverflowAsync(LayoutElement element);
        Task AdjustTextFlowAsync(LayoutElement element);
        
        Task<PageLayout> GetPageLayoutAsync(int issueId, int pageNumber);
        Task<List<PageLayout>> GetIssueLayoutsAsync(int issueId);
        Task<PageLayout> ConfigurePageLayoutAsync(int pageLayoutId, PageLayoutConfiguration config);
        Task<bool> ValidateLayoutConfigurationAsync(PageLayoutConfiguration config);
        Task<List<string>> CheckLayoutConflictsAsync(int pageLayoutId);
        Task<List<Template>> GetAvailableTemplatesAsync();
        Task<LayoutElement> PlaceArticlePartOnLayoutBlockAsync(LayoutBlockInfo blockInfo, int articleId, double height);
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
    
    // Класс для отслеживания информации о блоке макета (колонке)
    public class LayoutBlockInfo
    {
        public int PageLayoutId { get; set; }
        public int ColumnIndex { get; set; }
        public double CurrentYOffset { get; set; } = 0.5; // Текущее смещение Y в колонке (начальное значение - базовый отступ)
        public double ColumnWidth { get; set; }
        public double ColumnGap { get; set; } // Промежуток между колонками
        public double AvailableHeight { get; set; }
    }
}