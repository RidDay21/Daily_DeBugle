namespace DailyDeBugle.Models
{
    public class Template
    {
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LayoutSettings { get; set; } = "{}"; // JSON с настройками шаблона
        public bool IsActive { get; set; } = true;
        
        // Навигационные свойства
        public List<PageLayout> PageLayouts { get; set; } = new();
    }
}