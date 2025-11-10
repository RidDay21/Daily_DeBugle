using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IIssueService
    {
        Task<List<Issue>> GetIssuesAsync(int publicationId);
        Task<Issue?> GetIssueAsync(int id);
        Task<Issue> CreateIssueAsync(Issue issue);
        Task<Issue> UpdateIssueAsync(Issue issue);
        Task DeleteIssueAsync(int id);
        Task<bool> PublishIssueAsync(int id);
    }
}