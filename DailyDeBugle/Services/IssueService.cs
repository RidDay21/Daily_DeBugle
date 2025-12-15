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
			bool exists = await _context.Issues
        		.AnyAsync(i => i.IssueNumber == issue.IssueNumber && 
                       		i.PublicationId == issue.PublicationId);

    		if (exists)
    		{
        		throw new InvalidOperationException($"Issue number '{issue.IssueNumber}' already exists for this publication.");
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
        
        public async Task<Issue> GetFeaturedIssueAsync()
        {
	        return await _context.Issues
		        .Include(i => i.Articles)
		        .Include(i => i.PageLayouts)
		        .Where(i => i.Status == IssueStatus.Published && i.IsFeatured)
		        .OrderByDescending(i => i.IssueDate)
		        .FirstOrDefaultAsync();
        }
    
        public async Task<List<Issue>> GetRecentIssuesAsync(int count)
        {
	        return await _context.Issues
		        .Include(i => i.Articles)
		        .Include(i => i.PageLayouts)
		        .Where(i => i.Status == IssueStatus.Published)
		        .OrderByDescending(i => i.IssueDate)
		        .Take(count)
		        .ToListAsync();
        }
    
        public async Task<int> GetTotalIssuesCountAsync()
        {
	        return await _context.Issues.CountAsync();
        }
    
        public async Task<int> GetPublishedIssuesCountAsync()
        {
	        return await _context.Issues.CountAsync(i => i.Status == IssueStatus.Published);
        }
        
        
    
        public async Task<bool> SetAsFeaturedIssueAsync(int issueId)
        {
	        var issue = await _context.Issues.FindAsync(issueId);
	        if (issue == null) return false;
        
	        // Снимаем выделение со всех выпусков
	        await _context.Issues
		        .Where(i => i.IsFeatured)
		        .ExecuteUpdateAsync(setters => setters.SetProperty(i => i.IsFeatured, false));
        
	        // Выделяем выбранный выпуск
	        issue.IsFeatured = true;
	        await _context.SaveChangesAsync();
        
	        return true;
        }
        
        public async Task<bool> RemoveFromFeaturedAsync(int issueId)
        {
	        var issue = await _context.Issues.FindAsync(issueId);
	        if (issue == null) return false;
    
	        issue.IsFeatured = false;
	        await _context.SaveChangesAsync();
    
	        return true;
        }
        
        

    
        public async Task DownloadIssueAsPdfAsync(int issueId)
        {
	        var issue = await GetIssueAsync(issueId);
	        // Здесь будет логика генерации PDF
	        // Пока что просто возвращаем сообщение
        }
    }
}
