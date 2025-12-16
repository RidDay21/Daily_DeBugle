using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IIssueService
    {
        Task<List<Issue>> GetAllAsync();
        Task<List<Issue>> GetIssuesAsync(int publicationId);
        Task<Issue?> GetIssueAsync(int id);
        Task<Issue> CreateAsync(Issue issue);
        Task<Issue> UpdateAsync(Issue issue);
        Task DeleteAsync(int id);
        Task<bool> PublishAsync(int id);
        Task<List<Publication>> GetPublicationsAsync();
        Task<Issue> GetFeaturedIssueAsync();
        Task<List<Issue>> GetRecentIssuesAsync(int count);
        Task<int> GetTotalIssuesCountAsync();
        Task<int> GetPublishedIssuesCountAsync();
        Task<bool> SetAsFeaturedIssueAsync(int issueId);
        Task<bool> RemoveFromFeaturedAsync(int issueId);
        Task<bool> ApplyTemplateToIssueAsync(int issueId, int templateId);
    }
}