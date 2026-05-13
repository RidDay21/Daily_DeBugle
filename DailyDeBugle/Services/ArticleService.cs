using DailyDeBugle.Data;
using DailyDeBugle.Helpers;
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
                .Include(a => a.Images)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }
        
        public async Task<List<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Issue)
                .Include(a => a.Images)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Article>> GetArticlesByIssueAsync(int issueId)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Where(a => a.IssueId == issueId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<Article?> GetArticleAsync(int id)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Issue)
                .Include(a => a.Images)
                .Include(a => a.Comments)
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

        public async Task<bool> DeleteArticleAsync(int id)
        {
            try
            {
                var article = await _context.Articles
                    .Include(a => a.Comments)
                    .Include(a => a.LayoutElements)
                    .Include(a => a.ArticleParts)
                    .Include(a => a.Images)
                    .FirstOrDefaultAsync(a => a.ArticleId == id);
                
                if (article == null)
                {
                    return false;
                }

                // Удаляем связанные комментарии
                if (article.Comments.Any())
                {
                    _context.Comments.RemoveRange(article.Comments);
                }
                
                // Удаляем связанные элементы макета
                if (article.LayoutElements.Any())
                {
                    _context.LayoutElements.RemoveRange(article.LayoutElements);
                }
                
                // Удаляем части статьи
                if (article.ArticleParts.Any())
                {
                    _context.ArticleParts.RemoveRange(article.ArticleParts);
                }

                // Удаляем картинки статьи
                if (article.Images.Any())
                {
                    foreach (var img in article.Images)
                    {
                        FileUploadHelper.DeleteFile(img.ImagePath);
                    }
                    _context.ArticleImages.RemoveRange(article.Images);
                }
                
                // Обнуляем связи с продолжениями
                if (article.ContinuedFromArticleId.HasValue)
                {
                    var originalArticle = await _context.Articles
                        .FirstOrDefaultAsync(a => a.ArticleId == article.ContinuedFromArticleId.Value);
                    if (originalArticle != null)
                    {
                        originalArticle.HasContinuation = false;
                        originalArticle.ContinuedOnArticleId = null;
                    }
                }
                
                if (article.ContinuedOnArticleId.HasValue)
                {
                    var continuation = await _context.Articles
                        .FirstOrDefaultAsync(a => a.ArticleId == article.ContinuedOnArticleId.Value);
                    if (continuation != null)
                    {
                        continuation.ContinuedFromArticleId = null;
                    }
                }
                
                // Удаляем саму статью
                _context.Articles.Remove(article);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting article #{id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<List<Article>> GetArticlesByStatusAsync(ArticleStatus status)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<ArticleImage>> GetImagesAsync(int articleId)
        {
            return await _context.ArticleImages
                .Where(i => i.ArticleId == articleId)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.ArticleImageId)
                .ToListAsync();
        }

        public async Task<ArticleImage> AddImageAsync(int articleId, string imagePath, string? caption = null, int sortOrder = 0)
        {
            var image = new ArticleImage
            {
                ArticleId = articleId,
                ImagePath = imagePath,
                Caption = caption,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.ArticleImages.Add(image);
            await _context.SaveChangesAsync();
            return image;
        }

        public async Task<bool> DeleteImageAsync(int articleImageId)
        {
            var image = await _context.ArticleImages.FirstOrDefaultAsync(i => i.ArticleImageId == articleImageId);
            if (image == null) return false;

            FileUploadHelper.DeleteFile(image.ImagePath);
            _context.ArticleImages.Remove(image);
            await _context.SaveChangesAsync();
            return true;
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

            if (!string.IsNullOrWhiteSpace(comments))
            {
                _context.Comments.Add(new Comment
                {
                    ArticleId = articleId,
                    Content = comments.Trim(),
                    CreatedDate = DateTime.UtcNow,
                    IsEditorComment = true
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectArticleAsync(int articleId, string reason)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null) return false;

            article.Status = ArticleStatus.Rejected;
            article.ModifiedDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                _context.Comments.Add(new Comment
                {
                    ArticleId = articleId,
                    Content = "[Отклонено] " + reason.Trim(),
                    CreatedDate = DateTime.UtcNow,
                    IsEditorComment = true
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Comment> AddEditorCommentAsync(int articleId, string content)
        {
            var article = await _context.Articles.FindAsync(articleId)
                ?? throw new InvalidOperationException($"Article {articleId} not found");

            var comment = new Comment
            {
                ArticleId = articleId,
                Content = content.Trim(),
                CreatedDate = DateTime.UtcNow,
                IsEditorComment = true
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
        
        
        /// <summary>
        /// Создать продолжение статьи
        /// </summary>
        public async Task<Article> CreateContinuationAsync(int originalArticleId, string continuationContent)
        {
            var originalArticle = await _context.Articles
                .Include(a => a.Issue)
                .FirstOrDefaultAsync(a => a.ArticleId == originalArticleId);
            
            if (originalArticle == null)
                throw new ArgumentException($"Article with ID {originalArticleId} not found");

            var continuation = new Article
            {
                Headline = originalArticle.Headline + " (продолжение)",
                Content = continuationContent,
                AuthorId = originalArticle.AuthorId,
                IssueId = originalArticle.IssueId,
                Status = originalArticle.Status,
                ContinuedFromArticleId = originalArticleId,
                StartPage = originalArticle.StartPage,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // Обновляем оригинальную статью
            originalArticle.HasContinuation = true;
            originalArticle.ContinuedOnArticleId = continuation.ArticleId; // Будет установлен после сохранения
            
            _context.Articles.Add(continuation);
            await _context.SaveChangesAsync();
            
            // Обновляем ссылку на продолжение
            originalArticle.ContinuedOnArticleId = continuation.ArticleId;
            await _context.SaveChangesAsync();

            return continuation;
        }

        /// <summary>
        /// Рассчитать статистику статьи (кол-во слов, символов и т.д.)
        /// </summary>
        public async Task CalculateArticleStatisticsAsync(int articleId)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null) return;

            // Расчет статистики
            article.CharacterCount = article.Content?.Length ?? 0;
            article.WordCount = string.IsNullOrWhiteSpace(article.Content) ? 0 : 
                article.Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            article.ParagraphCount = string.IsNullOrWhiteSpace(article.Content) ? 0 : 
                article.Content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
            
            // Расчет высоты (очень приблизительно)
            article.EstimatedHeightCm = (int)Math.Ceiling(
                (article.ParagraphCount * 2.0) + 
                (article.CharacterCount / 1000.0 * 5.0)
            );
            
            if (article.EstimatedHeightCm < 3) article.EstimatedHeightCm = 3;
            
            article.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Разбить статью на части для пагинации
        /// </summary>
        public async Task<List<ArticlePart>> SplitArticleForPaginationAsync(int articleId, int maxCharsPerPart)
        {
            var article = await _context.Articles
                .Include(a => a.ArticleParts)
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);
            
            if (article == null || string.IsNullOrEmpty(article.Content))
                return new List<ArticlePart>();

            // Удаляем старые части
            if (article.ArticleParts.Any())
            {
                _context.ArticleParts.RemoveRange(article.ArticleParts);
            }

            var parts = new List<ArticlePart>();
            
            if (article.Content.Length <= maxCharsPerPart)
            {
                // Статья целиком помещается в одну часть
                parts.Add(new ArticlePart
                {
                    ArticleId = articleId,
                    ContentPart = article.Content,
                    PartNumber = 1,
                    TotalParts = 1,
                    IsBeginning = true,
                    IsEnding = true,
                    CreatedDate = DateTime.UtcNow
                });
            }
            else
            {
                // Разбиваем статью на части
                string remaining = article.Content;
                int partNumber = 1;
                
                while (!string.IsNullOrEmpty(remaining))
                {
                    int breakPoint = FindGoodBreakPoint(remaining, maxCharsPerPart);
                    string partContent = remaining.Substring(0, breakPoint);
                    remaining = remaining.Substring(breakPoint);
                    
                    parts.Add(new ArticlePart
                    {
                        ArticleId = articleId,
                        ContentPart = partContent + (string.IsNullOrEmpty(remaining) ? "" : " [→]"),
                        PartNumber = partNumber,
                        TotalParts = 0, // Будет обновлено позже
                        IsBeginning = partNumber == 1,
                        IsEnding = string.IsNullOrEmpty(remaining),
                        CreatedDate = DateTime.UtcNow
                    });
                    
                    partNumber++;
                }
                
                // Обновляем TotalParts для всех частей
                foreach (var part in parts)
                {
                    part.TotalParts = parts.Count;
                }
            }

            // Сохраняем части в базе
            _context.ArticleParts.AddRange(parts);
            await _context.SaveChangesAsync();

            return parts;
        }

        /// <summary>
        /// Найти хорошую точку разрыва для разбиения статьи
        /// </summary>
        private int FindGoodBreakPoint(string text, int maxChars)
        {
            if (text.Length <= maxChars) return text.Length;
            
            // Ищем хорошее место для разрыва
            // 1. Сначала ищем конец абзаца
            int paragraphBreak = text.LastIndexOf("\n\n", Math.Min(maxChars, text.Length - 2));
            if (paragraphBreak > maxChars * 0.7) // Если нашли не слишком близко к началу
            {
                return paragraphBreak + 2;
            }
            
            // 2. Ищем конец предложения
            int sentenceBreak = Math.Max(
                text.LastIndexOf(". ", Math.Min(maxChars, text.Length - 2)),
                Math.Max(
                    text.LastIndexOf("! ", Math.Min(maxChars, text.Length - 2)),
                    text.LastIndexOf("? ", Math.Min(maxChars, text.Length - 2))
                )
            );
            
            if (sentenceBreak > maxChars * 0.7)
            {
                return sentenceBreak + 2;
            }
            
            // 3. Ищем ближайший пробел
            int spaceBreak = text.LastIndexOf(' ', Math.Min(maxChars, text.Length - 1));
            if (spaceBreak > maxChars * 0.7)
            {
                return spaceBreak + 1;
            }
            
            // 4. Просто обрезаем
            return maxChars;
        }

        /// <summary>
        /// Получить все продолжения статьи
        /// </summary>
        public async Task<List<Article>> GetArticleContinuationsAsync(int articleId)
        {
            return await _context.Articles
                .Where(a => a.ContinuedFromArticleId == articleId)
                .OrderBy(a => a.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Получить цепочку статьи (оригинал + все продолжения)
        /// </summary>
        public async Task<List<Article>> GetArticleChainAsync(int articleId)
        {
            var chain = new List<Article>();

            // Начинаем с текущей статьи
            var current = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);

            if (current == null) return chain;

            // Находим начало цепочки (первую статью)
            while (current.ContinuedFromArticleId.HasValue)
            {
                current = await _context.Articles
                    .Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.ArticleId == current.ContinuedFromArticleId.Value);

                if (current == null) break;
            }

            // Собираем всю цепочку
            while (current != null)
            {
                chain.Add(current);

                if (!current.ContinuedOnArticleId.HasValue)
                    break;

                current = await _context.Articles
                    .Include(a => a.Author)
                    .FirstOrDefaultAsync(a => a.ArticleId == current.ContinuedOnArticleId.Value);
            }

            return chain;
        }
    }
}

