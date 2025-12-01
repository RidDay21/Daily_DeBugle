using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
    public class ArticleService : IArticleService
    {
        private readonly ApplicationDbContext _context;

        public ArticleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Article>> GetArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Issue)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<List<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Issue)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Article>> GetArticlesByIssueAsync(int issueId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.IssueId == issueId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Issue)
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            article.CreatedDate = DateTime.UtcNow;
            article.ModifiedDate = DateTime.UtcNow;
            article.Status = ArticleStatus.Draft;
            Console.WriteLine($"[DEBUG] Creating article: AuthorId={article.AuthorId}, IssueId={article.IssueId}, Headline={article.Headline}");
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article> UpdateArticleAsync(Article article)
        {
            article.ModifiedDate = DateTime.UtcNow;
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Article>> GetArticlesByStatusAsync(ArticleStatus status)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveArticleAsync(int articleId, string editedContent)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null) return false;

            article.Content = editedContent;
            article.Status = ArticleStatus.Approved;
            article.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendForRevisionAsync(int articleId, string comments)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null) return false;

            article.Status = ArticleStatus.RequiresRevision;
            article.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectArticleAsync(int articleId, string reason)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null) return false;

            article.Status = ArticleStatus.Rejected;
            article.ModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

