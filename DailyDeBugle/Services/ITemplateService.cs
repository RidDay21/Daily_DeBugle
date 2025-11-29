using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services
{
    public interface ITemplateService
    {
        Task<List<Template>> GetTemplatesAsync();
        Task<Template> GetTemplateByIdAsync(int id);
        Task<Template> CreateTemplateAsync(Template template);
    }

}