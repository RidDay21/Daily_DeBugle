// DailyDeBugle/Services/ITextMeasureService.cs
using DailyDeBugle.Models;

namespace DailyDeBugle.Services
{
    public interface ITextMeasureService
    {
        Task<TextMeasurement> MeasureTextAsync(TextMeasurementOptions options);
        Task<int> FindBreakPointAsync(string text, double availableHeight, double columnWidthCm, bool isFirstPage = true);
        Task<double> CalculateTextHeightAsync(string text, double columnWidthCm);
    }
}