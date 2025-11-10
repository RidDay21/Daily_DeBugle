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
    }
}