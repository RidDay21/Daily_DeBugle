namespace DailyDeBugle.Models
{
    public class TextStyle
    {
        public string PrimaryFont { get; set; } = "Times New Roman";
        public string HeadingFont { get; set; } = "Times New Roman";
        
        public int H1Size { get; set; } = 24;
        public int H2Size { get; set; } = 18;
        public int BodySize { get; set; } = 12;
        
        public double BodyLineSpacing { get; set; } = 1.2;
        public double HeadingLineSpacing { get; set; } = 1.1;
        
        public int ColumnCount { get; set; } = 3;
        public double ColumnGap { get; set; } = 0.5;
        public double ColumnWidth { get; set; } = 8;
    }
}