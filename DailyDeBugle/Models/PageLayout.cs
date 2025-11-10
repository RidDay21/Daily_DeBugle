namespace DailyDeBugle.Models
{
    public class PageLayout
    {
        public int PageLayoutId { get; set; }
        public int PageNumber { get; set; }
        public string LayoutSettings { get; set; } = "{}"; // JSON с настройками
        
        // Внешние ключи
        public int IssueId { get; set; }
        public Issue Issue { get; set; } = null!;
        public int TemplateId { get; set; }
        public Template Template { get; set; } = null!;
        
        // Навигационные свойства
        public List<LayoutElement> LayoutElements { get; set; } = new();
    }
}