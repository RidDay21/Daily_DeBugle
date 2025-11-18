using System.ComponentModel.DataAnnotations.Schema;

namespace DailyDeBugle.Models
{
    public class Issue
    {
        public int IssueId { get; set; }
        public string IssueNumber { get; set; } = string.Empty;
        
        [Column(TypeName = "timestamp without time zone")]
        public DateTime IssueDate { get; set; }
        public string? CoverImagePath { get; set; }
        public IssueStatus Status { get; set; } = IssueStatus.InProgress;
        
        // Внешние ключи
        public int PublicationId { get; set; }
        public Publication Publication { get; set; } = null!;
        
        // Навигационные свойства
        public List<Article> Articles { get; set; } = new();
        public List<PageLayout> PageLayouts { get; set; } = new();
    }
}