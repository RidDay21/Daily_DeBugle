using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface ITemplateService
    {
        Task<List<Template>> GetTemplatesAsync();
        Task<Template> GetTemplateByIdAsync(int id);
        Task<Template> CreateTemplateAsync(Template template);
        Task<Template> UpdateTemplateAsync(Template template); // Добавьте
        Task DeleteTemplateAsync(int id); // Добавьте
        Task<int> GetPageLayoutCountForTemplateAsync(int templateId); // Добавьте
    }
}