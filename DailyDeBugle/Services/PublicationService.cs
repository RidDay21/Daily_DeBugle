
using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
    public class PublicationService : IPublicationService
    {
        private readonly ApplicationDbContext _context;

        public PublicationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Publication>> GetPublicationsAsync()
        {
            return await _context.Publications
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Publication?> GetPublicationAsync(int id)
        {
            return await _context.Publications
                .Include(p => p.Issues)
                .FirstOrDefaultAsync(p => p.PublicationId == id);
        }

        public async Task<Publication> CreatePublicationAsync(Publication publication)
        {
            // проверка на наличие публикации с таким именем
            bool exists = await _context.Publications
                .AnyAsync(p => p.Name == publication.Name && p.IsActive);

            if (exists)
                throw new InvalidOperationException("A publication with this name already exists.");

            publication.IsActive = true;
            publication.CreatedDate = DateTime.UtcNow;

            _context.Publications.Add(publication);
            await _context.SaveChangesAsync();
            return publication;
        }

        public async Task<Publication> UpdatePublicationAsync(Publication publication)
        {
            // Проверка на дублирование названия
            bool nameExists = await _context.Publications
                .Where(p => p.IsActive)
                .Where(p => p.Name == publication.Name)
                .AnyAsync(p => p.PublicationId != publication.PublicationId);

            if (nameExists)
            {
                throw new InvalidOperationException("A publication with this name already exists.");
            }

            // Обновление и сохранение
            _context.Entry(publication).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return publication;
        }

        public async Task DeletePublicationAsync(int id)
        {
            var publication = await _context.Publications
                .Include(p => p.Issues)
                .ThenInclude(i => i.Articles)
                .FirstOrDefaultAsync(p => p.PublicationId == id);

            if (publication == null)
            {
                return; 
            }
            
            var allArticlesToDelete = publication.Issues
                .SelectMany(i => i.Articles) 
                .ToList();
            
            if (allArticlesToDelete.Any())
            {
                _context.Articles.RemoveRange(allArticlesToDelete);
            }
            if (publication.Issues.Any())
            {
                _context.Issues.RemoveRange(publication.Issues);
            }
            
            publication.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
}