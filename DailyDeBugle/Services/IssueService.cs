using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
    public class IssueService : IIssueService
    {
        private readonly ApplicationDbContext _context;

        public IssueService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Issue>> GetAllAsync()
        {
            return await _context.Issues.Include(i => i.Publication)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<List<Issue>> GetIssuesAsync(int publicationId)
        {
            return await _context.Issues
                .Include(i => i.Publication)
                .Where(i => i.PublicationId == publicationId)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<Issue?> GetIssueAsync(int id)
        {
            return await _context.Issues
                .Include(i => i.Publication)
                .FirstOrDefaultAsync(i => i.IssueId == id);
        }

        public async Task<Issue> CreateAsync(Issue issue)
        {
            bool exists = await _context.Issues
                .AnyAsync(i => i.IssueNumber == issue.IssueNumber &&
                               i.PublicationId == issue.PublicationId);

            if (exists)
            {
                throw new InvalidOperationException(
                    $"Issue number '{issue.IssueNumber}' already exists for this publication.");
            }

            if (issue.IssueDate.Kind == DateTimeKind.Unspecified)
            {
                issue.IssueDate = DateTime.SpecifyKind(issue.IssueDate, DateTimeKind.Local).ToUniversalTime();
            }
            else
            {
                issue.IssueDate = issue.IssueDate.ToUniversalTime();
            }

            issue.Status = IssueStatus.InProgress;
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            return issue;
        }

        public async Task<Issue> UpdateAsync(Issue issue)
        {
            if (issue.IssueDate.Kind == DateTimeKind.Unspecified)
                issue.IssueDate = DateTime.SpecifyKind(issue.IssueDate, DateTimeKind.Local).ToUniversalTime();
            else
                issue.IssueDate = issue.IssueDate.ToUniversalTime();

            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();
            return issue;
        }

        public async Task DeleteAsync(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue != null)
            {
                _context.Issues.Remove(issue);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PublishAsync(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null)
                return false;

            issue.Status = IssueStatus.Published;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Publication>> GetPublicationsAsync()
        {
            return await _context.Publications
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // UC-13: Редактирование выпуска
        public async Task<Issue?> UpdateIssueDetailsAsync(int id, string issueNumber, DateTime issueDate)
        {
            var issue = await _context.Issues
                .Include(i => i.Publication)
                .FirstOrDefaultAsync(i => i.IssueId == id);

            if (issue == null || issue.Status != IssueStatus.InProgress)
                return null;

            // Проверка уникальности номера выпуска в рамках публикации
            var existingIssue = await _context.Issues
                .Where(i => i.PublicationId == issue.PublicationId &&
                            i.IssueNumber == issueNumber &&
                            i.IssueId != id)
                .FirstOrDefaultAsync();

            if (existingIssue != null)
                throw new InvalidOperationException("Issue number must be unique within publication");

            issue.IssueNumber = issueNumber;
            issue.IssueDate = issueDate;

            await _context.SaveChangesAsync();
            return issue;
        }

        public async Task<bool> CanEditIssueAsync(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            return issue?.Status == IssueStatus.InProgress;
        }

        public async Task<bool> DeleteIssueWithContentAsync(int id)
        {
            var issue = await _context.Issues
                .Include(i => i.Articles)
                .Include(i => i.PageLayouts)
                .FirstOrDefaultAsync(i => i.IssueId == id);

            if (issue == null)
                return false;

            // Удаляем файл обложки
            if (!string.IsNullOrEmpty(issue.CoverImagePath))
            {
                await DeleteCoverImage(issue.CoverImagePath);
            }

            // Удаляем все статьи выпуска
            _context.Articles.RemoveRange(issue.Articles);

            // Удаляем все layouts выпуска
            _context.PageLayouts.RemoveRange(issue.PageLayouts);

            // Удаляем сам выпуск
            _context.Issues.Remove(issue);

            await _context.SaveChangesAsync();
            return true;
        }

// Добавим метод удаления обложки
        private async Task DeleteCoverImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            try
            {
                var filePath = Path.Combine("wwwroot", imagePath.TrimStart('/'));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение
                Console.WriteLine($"Error deleting cover image: {ex.Message}");
            }
        }
    }
}
