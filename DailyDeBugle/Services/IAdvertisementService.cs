using DailyDeBugle.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DailyDeBugle.Services;

public interface IAdvertisementService
{
    Task<List<AdvertisementBlock>> GetAvailableAdsAsync();
    Task SaveAdPlacementAsync(int issueId, List<AdvertisementPlacement> placedAds);
}
    