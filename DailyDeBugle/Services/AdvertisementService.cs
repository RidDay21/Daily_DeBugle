using DailyDeBugle.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyDeBugle.Services
{
    public class AdvertisementService : IAdvertisementService
    {
        public Task<List<AdvertisementBlock>> GetAvailableAdsAsync()
        {
            var today = DateTime.UtcNow.Date; // ← ИСПРАВЛЕНО: используем UTC
            
            var ads = new List<AdvertisementBlock>
            {
                new AdvertisementBlock
                {
                    AdvertisementBlockId = 1,
                    Advertiser = "Coca-Cola",
                    Content = "https://example.com/coke-ad.jpg",
                    StartDate = DateTime.UtcNow.AddDays(-10), // ← ИСПРАВЛЕНО
                    EndDate = DateTime.UtcNow.AddDays(20)     // ← ИСПРАВЛЕНО
                },
                new AdvertisementBlock
                {
                    AdvertisementBlockId = 2,
                    Advertiser = "Nike",
                    Content = "Just do it! Great sports equipment for professionals and amateurs.",
                    StartDate = DateTime.UtcNow.AddDays(-5),  // ← ИСПРАВЛЕНО
                    EndDate = DateTime.UtcNow.AddDays(25)     // ← ИСПРАВЛЕНО
                },
                new AdvertisementBlock
                {
                    AdvertisementBlockId = 3,
                    Advertiser = "Apple",
                    Content = "https://example.com/iphone-ad.png",
                    StartDate = DateTime.UtcNow.AddDays(-2),  // ← ИСПРАВЛЕНО
                    EndDate = DateTime.UtcNow.AddDays(30)     // ← ИСПРАВЛЕНО
                }
            };

            // Фильтруем по активным рекламным кампаниям
            var activeAds = ads.Where(a => a.StartDate <= today && a.EndDate >= today).ToList();
            
            return Task.FromResult(activeAds);
        }

        public Task SaveAdPlacementAsync(int issueId, List<AdvertisementPlacement> placedAds)
        {
            Console.WriteLine($"Saving {placedAds.Count} ads for issue {issueId}");
            return Task.CompletedTask;
        }
    }
}