using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface IIssueService
    {
        // Получение всех выпусков
        Task<List<Issue>> GetAllAsync();

        // Получение всех выпусков конкретной публикации
        Task<List<Issue>> GetIssuesAsync(int publicationId);

        // Получение выпуска по ID
        Task<Issue?> GetIssueAsync(int id);

        // Создание нового выпуска (UC-01)
        Task<Issue> CreateAsync(Issue issue);

        // Обновление выпуска
        Task<Issue> UpdateAsync(Issue issue);

        // Удаление выпуска
        Task DeleteAsync(int id);

        // Публикация выпуска
        Task<bool> PublishAsync(int id);

        // Получение списка публикаций для выбора при создании выпуска
        Task<List<Publication>> GetPublicationsAsync();
    }
}