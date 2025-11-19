namespace DailyDeBugle.Services;

public interface IGlobalTextStyleService
{
    Task<GlobalTextStyle> GetStylesForIssueAsync(int issueId);
    Task<GlobalTextStyle> SaveStylesAsync(GlobalTextStyle styles);
    Task ApplyStylesToIssueAsync(int issueId);
}