using DailyDeBugle.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace DailyDeBugle.Services
{
    public interface IAdvertisementService
    {
        // Основные CRUD операции
        Task<AdvertisementBlock> CreateAdAsync(AdvertisementBlock ad);
        Task<AdvertisementBlock?> GetAdByIdAsync(int id);
        Task<AdvertisementBlock> UpdateAdAsync(AdvertisementBlock ad);
        Task DeleteAdAsync(int id);
        
        // Получение списков рекламы
        Task<List<AdvertisementBlock>> GetAvailableAdsAsync();
        Task<List<AdvertisementBlock>> SearchAdsAsync(string searchTerm);
        Task<List<AdvertisementBlock>> GetExpiredAdsAsync();
        
        // Работа с файлами
        Task<string> UploadAdImageAsync(IBrowserFile file);
        
        // Работа с размещениями
        Task SaveAdPlacementAsync(int issueId, List<AdvertisementPlacementDto> placedAds);
        Task<bool> IsAdUsedInLayoutsAsync(int adId);
        Task<List<LayoutElement>> GetAdPlacementsAsync(int adId);
        
        // Вспомогательные методы
        Task CleanupExpiredAdsAsync();
    }
}