using DailyDeBugle.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyDeBugle.Services
{
    public interface IAdvertisementService
    {
        Task<List<AdvertisementBlock>> GetAvailableAdsAsync();
        Task SaveAdPlacementAsync(int issueId, List<AdvertisementPlacement> placedAds);
    }
    
    // Выносим модель размещения рекламы в отдельный класс в том же файле
    public class AdvertisementPlacement
    {
        public AdvertisementBlock AdvertisementBlock { get; set; } = new AdvertisementBlock();
        public int PageNumber { get; set; } = 1;
        public string TextFlow { get; set; } = "Right";
        public int Width { get; set; }
        public int Height { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}