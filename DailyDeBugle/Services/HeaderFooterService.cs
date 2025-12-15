using DailyDeBugle.Data;

using DailyDeBugle.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyDeBugle.Services;

public class HeaderFooterService : IHeaderFooterService
{
    private readonly ApplicationDbContext _context;
    private readonly IIssueService _issueService;
    private readonly IPublicationService _publicationService;

    public HeaderFooterService(ApplicationDbContext context, IIssueService issueService, IPublicationService publicationService)
    {
        _context = context;
        _issueService = issueService;
        _publicationService = publicationService;
    }

    public async Task<HeaderFooterSettings> GetSettingsForIssueAsync(int issueId)
    {
        var settings = await _context.HeaderFooterSettings
            .Include(hf => hf.Issue)
            .FirstOrDefaultAsync(hf => hf.IssueId == issueId);

        if (settings == null)
        {
            // Create default settings
            settings = new HeaderFooterSettings
            {
                IssueId = issueId,
                HeaderLeft = "{PublicationName}",
                HeaderCenter = "Выпуск №{IssueNumber}",
                HeaderRight = "{IssueDate}",
                FooterLeft = "Контакт: {ContactEmail}",
                FooterCenter = "Страница {PageNumber}",
                FooterRight = "{CurrentDate}"
            };
            
            _context.HeaderFooterSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<HeaderFooterSettings> SaveSettingsAsync(HeaderFooterSettings settings)
    {
        var existing = await _context.HeaderFooterSettings
            .FirstOrDefaultAsync(hf => hf.Id == settings.Id);

        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(settings);
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            settings.CreatedAt = DateTime.UtcNow;
            settings.UpdatedAt = DateTime.UtcNow;
            _context.HeaderFooterSettings.Add(settings);
        }

        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<string> RenderHeaderAsync(int issueId, int pageNumber = 1)
    {
        var settings = await GetSettingsForIssueAsync(issueId);
        if (!settings.HeaderEnabled) return string.Empty;

        var issue = await _issueService.GetIssueAsync(issueId);
        var publication = await _publicationService.GetPublicationAsync(issue.PublicationId);

        var header = $@"
            <div class='header' style='font-family: {settings.FontFamily}; font-size: {settings.FontSize}pt; text-align: {settings.Alignment};'>
                <table style='width: 100%;'>
                    <tr>
                        <td style='text-align: left;'>{await ReplacePlaceholders(settings.HeaderLeft, issue, publication, pageNumber)}</td>
                        <td style='text-align: center;'>{await ReplacePlaceholders(settings.HeaderCenter, issue, publication, pageNumber)}</td>
                        <td style='text-align: right;'>{await ReplacePlaceholders(settings.HeaderRight, issue, publication, pageNumber)}</td>
                    </tr>
                </table>
            </div>";

        return header;
    }

    public async Task<string> RenderFooterAsync(int issueId, int pageNumber = 1)
    {
        var settings = await GetSettingsForIssueAsync(issueId);
        if (!settings.FooterEnabled) return string.Empty;

        var issue = await _issueService.GetIssueAsync(issueId);
        var publication = await _publicationService.GetPublicationAsync(issue.PublicationId);

        var footer = $@"
            <div class='footer' style='font-family: {settings.FontFamily}; font-size: {settings.FontSize}pt; text-align: {settings.Alignment};'>
                <table style='width: 100%;'>
                    <tr>
                        <td style='text-align: left;'>{await ReplacePlaceholders(settings.FooterLeft, issue, publication, pageNumber)}</td>
                        <td style='text-align: center;'>{await ReplacePlaceholders(settings.FooterCenter, issue, publication, pageNumber)}</td>
                        <td style='text-align: right;'>{await ReplacePlaceholders(settings.FooterRight, issue, publication, pageNumber)}</td>
                    </tr>
                </table>
            </div>";

        return footer;
    }

    private async Task<string> ReplacePlaceholders(string template, Issue issue, Publication publication, int pageNumber)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        return template
            .Replace("{PublicationName}", publication.Name ?? "")
            .Replace("{IssueNumber}", issue.IssueNumber ?? "")
            .Replace("{IssueDate}", issue.IssueDate.ToString("dd.MM.yyyy"))
            .Replace("{PageNumber}", pageNumber.ToString())
            .Replace("{CurrentDate}", DateTime.Now.ToString("dd.MM.yyyy"))
            .Replace("{ContactEmail}", "ddbug@mail.ru")
            .Replace("{SectionName}", await GetSectionNameForPage(issue.IssueId, pageNumber) ?? "");
    }

    private async Task<string> GetSectionNameForPage(int issueId, int pageNumber)
    {
        // Implementation to get section name based on page number
        // This would query the layout data for the issue
        return "Основной раздел";
    }

    public async Task<List<HeaderFooterTemplate>> GetTemplatesAsync(string templateType = null)
    {
        var query = _context.HeaderFooterTemplates.AsQueryable();
        
        if (!string.IsNullOrEmpty(templateType))
        {
            query = query.Where(t => t.TemplateType == templateType);
        }

        return await query.Where(t => t.IsSystemTemplate).ToListAsync();
    }
}