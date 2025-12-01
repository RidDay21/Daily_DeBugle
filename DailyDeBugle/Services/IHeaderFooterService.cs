using DailyDeBugle.Models;

namespace DailyDeBugle.Services;

public interface IHeaderFooterService
{
    Task<HeaderFooterSettings> GetSettingsForIssueAsync(int issueId);
    Task<HeaderFooterSettings> SaveSettingsAsync(HeaderFooterSettings settings);
    Task<List<HeaderFooterTemplate>> GetTemplatesAsync(string templateType = null);
    Task<string> RenderHeaderAsync(int issueId, int pageNumber = 1);
    Task<string> RenderFooterAsync(int issueId, int pageNumber = 1);
}