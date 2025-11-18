using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface ILayoutService
    {
        Task CreateOrUpdatePageLayoutAsync(PageLayout layout);
        Task<PageLayout> GetPageLayoutAsync(int pageLayoutId);
        Task<List<PageLayout>> GetPageLayoutsForIssueAsync(int issueId);
        Task AddArticleToLayoutAsync(int pageLayoutId, int articleId, string position, string size);
        Task RemoveElementFromLayoutAsync(int layoutElementId);
        Task UpdateIssueStatusAsync(int issueId, IssueStatus status);
        Task<LayoutElement> UpdateLayoutElementAsync(LayoutElement element);
        Task ApplyTemplateAsync(int pageLayoutId, int templateId);
        Task<List<Template>> GetTemplatesAsync();
        Task<bool> CheckTextOverflowAsync(LayoutElement element);
        Task AdjustTextFlowAsync(LayoutElement element);
        Task<List<Article>> GetApprovedArticlesForIssueAsync(int issueId);
    }
}