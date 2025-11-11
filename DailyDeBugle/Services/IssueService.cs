using DailyDeBugle.Data;
using DailyDeBugle.Models;
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
            issue.Status = IssueStatus.InProgress;
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            return issue;
        }

        public async Task<Issue> UpdateAsync(Issue issue)
        {
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
    }
}
