using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace DailyDeBugle.Services
{
    public class AdvertisementService : IAdvertisementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdvertisementService> _logger;
        
        public AdvertisementService(
            ApplicationDbContext context,
            ILogger<AdvertisementService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<AdvertisementBlock> CreateAdAsync(AdvertisementBlock ad)
        {
            _context.AdvertisementBlocks.Add(ad);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Реклама создана: {ad.Advertiser} (ID: {ad.AdvertisementBlockId})");
            return ad;
        }
        
        public async Task<AdvertisementBlock?> GetAdByIdAsync(int id)
        {
            return await _context.AdvertisementBlocks.FindAsync(id);
        }
        
        public async Task<AdvertisementBlock> UpdateAdAsync(AdvertisementBlock ad)
        {
            _context.AdvertisementBlocks.Update(ad);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Реклама обновлена: ID {ad.AdvertisementBlockId}");
            return ad;
        }
        
        public async Task DeleteAdAsync(int id)
        {
            var ad = await _context.AdvertisementBlocks.FindAsync(id);
            if (ad != null)
            {
                _context.AdvertisementBlocks.Remove(ad);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Реклама удалена: ID {id}");
            }
        }
        
        public async Task<List<AdvertisementBlock>> GetAvailableAdsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.AdvertisementBlocks
                .Where(a => a.StartDate <= today && a.EndDate >= today)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();
        }
        
        public async Task<List<AdvertisementBlock>> SearchAdsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAvailableAdsAsync();
            
            var today = DateTime.UtcNow.Date;
            
            return await _context.AdvertisementBlocks
                .Where(a => a.StartDate <= today && a.EndDate >= today)
                .Where(a => a.Advertiser.Contains(searchTerm) || 
                           (a.Content != null && a.Content.Contains(searchTerm)))
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();
        }
        
        public async Task<List<AdvertisementBlock>> GetExpiredAdsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.AdvertisementBlocks
                .Where(a => a.EndDate < today)
                .OrderByDescending(a => a.EndDate)
                .ToListAsync();
        }
        
        public async Task<string> UploadAdImageAsync(IBrowserFile file)
        {
            return await Helpers.FileUploadHelper.UploadFileAsync(file, "ads");
        }
        
        public async Task SaveAdPlacementAsync(int issueId, List<AdvertisementPlacementDto> placedAds)
        {
            // Реализация сохранения размещений
            await Task.CompletedTask;
        }
        
        public async Task<bool> IsAdUsedInLayoutsAsync(int adId)
        {
            return await _context.LayoutElements
                .AnyAsync(le => le.AdvertisementBlockId == adId);
        }
        
        public async Task<List<LayoutElement>> GetAdPlacementsAsync(int adId)
        {
            return await _context.LayoutElements
                .Include(le => le.PageLayout)
                .ThenInclude(pl => pl.Issue)
                .Where(le => le.AdvertisementBlockId == adId)
                .ToListAsync();
        }
        
        public async Task CleanupExpiredAdsAsync()
        {
            var expiredAds = await GetExpiredAdsAsync();
            
            foreach (var ad in expiredAds)
            {
                await DeleteAdAsync(ad.AdvertisementBlockId);
            }
            
            _logger.LogInformation($"Очищено {expiredAds.Count} просроченных реклам");
        }
    }
}