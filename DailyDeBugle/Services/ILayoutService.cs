using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface ILayoutService
    {
        Task<LayoutElement?> GetLayoutElementAsync(int layoutElementId);
        Task CreateOrUpdatePageLayoutAsync(PageLayout layout);
        Task DeletePageLayoutAsync(int pageLayoutId);
        Task<PageLayout> GetPageLayoutAsync(int pageLayoutId);
        Task<List<PageLayout>> GetPageLayoutsForIssueAsync(int issueId);
        Task AddArticleToLayoutAsync(int pageLayoutId, int articleId, string position, string size);
        Task RemoveElementFromLayoutAsync(int layoutElementId);
        Task<LayoutElement> UpdateLayoutElementAsync(LayoutElement element);
        Task<PageLayout> GetPageLayoutAsync(int issueId, int pageNumber);
        Task<PageLayout> CreatePageLayoutAsync(PageLayout layout);
        Task<List<PageLayout>> GetIssueLayoutsAsync(int issueId);
        Task<List<string>> CheckLayoutConflictsAsync(int pageLayoutId);
        Task ApplyTemplateAsync(int pageLayoutId, int templateId);
        Task<bool> CheckTextOverflowAsync(LayoutElement element);
        Task AdjustTextFlowAsync(LayoutElement element);
        Task UpdateIssueStatusAsync(int issueId, IssueStatus status);
        Task<List<Article>> GetApprovedArticlesForIssueAsync(int issueId);
        Task<List<Template>> GetTemplatesAsync();
        Task<List<Template>> GetAvailableTemplatesAsync();
        Task CreateDefaultTemplateIfNotExists();
        Task AddAdvertisementToLayoutAsync(int pageLayoutId, int advertisementId, string position, string size, string textFlow = "None");
    }
}