using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
    public class IssueCommentService : IIssueCommentService
    {
        private const int MaxContentLength = 2000;
        private readonly ApplicationDbContext _context;

        public IssueCommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<IssueComment>> GetCommentsForIssueAsync(int issueId)
        {
            return await _context.IssueComments
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.IssueId == issueId)
                .OrderBy(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IssueComment> AddReaderCommentAsync(int issueId, int userId, string content)
        {
            var issue = await _context.Issues.FindAsync(issueId);
            if (issue == null)
                throw new InvalidOperationException("Выпуск не найден.");

            if (issue.Status != IssueStatus.Published)
                throw new InvalidOperationException("Комментарии доступны только к опубликованным выпускам.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("Пользователь не найден.");

            if (user.Role != UserRole.Reader)
                throw new UnauthorizedAccessException("Оставлять комментарии могут только читатели.");

            var trimmed = (content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw new ArgumentException("Текст комментария не может быть пустым.");

            if (trimmed.Length > MaxContentLength)
                throw new ArgumentException($"Комментарий не должен превышать {MaxContentLength} символов.");

            var comment = new IssueComment
            {
                IssueId = issueId,
                UserId = userId,
                Content = trimmed,
                CreatedDate = DateTime.UtcNow
            };

            _context.IssueComments.Add(comment);
            await _context.SaveChangesAsync();

            await _context.Entry(comment).Reference(c => c.User).LoadAsync();
            return comment;
        }
    }
}
