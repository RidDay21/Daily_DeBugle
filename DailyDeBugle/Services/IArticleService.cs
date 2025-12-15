using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IArticleService
    {
        // Существующие методы (оставляем как есть)
        Task<List<Article>> GetArticlesAsync();
        Task<List<Article>> GetArticlesByIssueAsync(int issueId);
        Task<Article?> GetArticleAsync(int id);
        Task<Article> CreateArticleAsync(Article article);
        Task<Article> UpdateArticleAsync(Article article);
        Task<List<Article>> GetAllArticlesAsync();
        Task DeleteArticleAsync(int id);

        /// <summary>
        /// Получить статьи по статусу (Under Review) 
        /// </summary>
        /// <returns></returns>

        Task<List<Article>> GetArticlesByStatusAsync(ArticleStatus status);

        /// <summary>
        /// Утвердить статью
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="editedContent"></param>
        /// <returns></returns>
        Task<bool> ApproveArticleAsync(int articleId, string editedContent);

        /// <summary>
        /// Для альтернативного сценария 1: Отправить на доработку
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="comments"></param>
        /// <returns></returns>
        Task<bool> SendForRevisionAsync(int articleId, string comments);

        /// <summary>
        /// // Для альтернативного сценария 2: Отклонить статью 
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        Task<bool> RejectArticleAsync(int articleId, string reason);
    }
}