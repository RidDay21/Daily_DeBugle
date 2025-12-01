namespace DailyDeBugle.Models
{
    public class Template
    {
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LayoutSettings { get; set; } = "{}"; // JSON с настройками шаблона
        
        public int DefaultColumnCount { get; set; } = 1;
        public double DefaultMarginTop { get; set; } = 1.0;
        public double DefaultMarginBottom { get; set; } = 1.0;
        public double DefaultMarginLeft { get; set; } = 1.0;
        public double DefaultMarginRight { get; set; } = 1.0;
        public double DefaultColumnGap { get; set; } = 0.5;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public List<PageLayout> PageLayouts { get; set; } = new();
    }
}