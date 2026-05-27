using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IIssueCommentService
    {
        Task<List<IssueComment>> GetCommentsForIssueAsync(int issueId);
        Task<IssueComment> AddReaderCommentAsync(int issueId, int userId, string content);
    }
}
