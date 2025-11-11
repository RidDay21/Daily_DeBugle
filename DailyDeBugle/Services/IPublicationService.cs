using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IPublicationService
    {
        Task<List<Publication>> GetPublicationsAsync();
        Task<Publication?> GetPublicationAsync(int id);
        Task<Publication> CreatePublicationAsync(Publication publication);
        Task<Publication> UpdatePublicationAsync(Publication publication);
        Task DeletePublicationAsync(int id);
    }
}