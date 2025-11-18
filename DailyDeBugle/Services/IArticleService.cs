using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IArticleService
    {
        Task<List<Article>> GetArticlesAsync();
        Task<List<Article>> GetArticlesByIssueAsync(int issueId);
        Task<Article?> GetArticleAsync(int id);
        Task<Article> CreateArticleAsync(Article article);
        Task<Article> UpdateArticleAsync(Article article);
        Task DeleteArticleAsync(int id);
    }
}