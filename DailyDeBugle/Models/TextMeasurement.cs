// DailyDeBugle/Models/TextMeasurement.cs
namespace DailyDeBugle.Models
{
    public class TextMeasurement
    {
        public double Width { get; set; }      // Ширина в пикселях
        public double Height { get; set; }     // Высота в пикселях
        public int LineCount { get; set; }     // Количество строк
        public int CharactersThatFit { get; set; } // Сколько символов поместилось
        public string? OverflowText { get; set; }  // Текст, который не поместился
    }

    public class TextMeasurementOptions
    {
        public string Text { get; set; } = string.Empty;
        public string FontFamily { get; set; } = "'Times New Roman', serif";
        public double FontSize { get; set; } = 12; // pt
        public double LineHeight { get; set; } = 1.2;
        public double MaxWidth { get; set; }       // px
        public double MaxHeight { get; set; }      // px (0 = без ограничения)
        public string FontWeight { get; set; } = "normal";
        public string FontStyle { get; set; } = "normal";
        public double Padding { get; set; } = 0;
    }
}