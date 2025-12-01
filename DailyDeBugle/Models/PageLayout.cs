namespace DailyDeBugle.Models
{
    public class PageLayout
    {
        public int PageLayoutId { get; set; }
        public int PageNumber { get; set; }
        public string LayoutSettings { get; set; } = "{}"; // JSON с настройками
        
        // Layout configuration
        public int ColumnCount { get; set; } = 1;
        public double MarginTop { get; set; } = 1.0;
        public double MarginBottom { get; set; } = 1.0;
        public double MarginLeft { get; set; } = 1.0;
        public double MarginRight { get; set; } = 1.0;
        public double ColumnGap { get; set; } = 0.5;
        public double TextAreaWidth { get; set; } = 8.0;
        public double TextAreaHeight { get; set; } = 10.0;
        public double ImageAreaWidth { get; set; } = 8.0;
        public double ImageAreaHeight { get; set; } = 6.0;
        
        // Внешние ключи
        public int IssueId { get; set; }
        public Issue Issue { get; set; } = null!;
        public int TemplateId { get; set; }
        public Template Template { get; set; } = null!;
        
        // Навигационные свойства
        public List<LayoutElement> LayoutElements { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}