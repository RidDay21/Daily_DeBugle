namespace DailyDeBugle.Models
{
    public class Publication
    {
        public int PublicationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Навигационные свойства
        public List<Issue> Issues { get; set; } = new();
        public List<User> Editors { get; set; } = new();
    }
}