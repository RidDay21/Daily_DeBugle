using DailyDeBugle.Data;
using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services;

public class TemplateService : ITemplateService
{
    private readonly ApplicationDbContext _context;

    public TemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Template>> GetTemplatesAsync()
    {
        return await _context.Templates
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Template> GetTemplateByIdAsync(int id)
    {
        return await _context.Templates.FindAsync(id);
    }

    public async Task<Template> CreateTemplateAsync(Template template)
    {
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    // Добавьте эти методы:

    public async Task<Template> UpdateTemplateAsync(Template template)
    {
        template.UpdatedAt = DateTime.UtcNow;
        _context.Templates.Update(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task DeleteTemplateAsync(int id)
    {
        var template = await _context.Templates.FindAsync(id);
        if (template != null)
        {
            _context.Templates.Remove(template);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetPageLayoutCountForTemplateAsync(int templateId)
    {
        return await _context.PageLayouts
            .CountAsync(p => p.TemplateId == templateId);
    }
}