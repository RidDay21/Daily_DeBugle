using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyDeBugle.Models
{
    public class Issue
    {
        public int IssueId { get; set; }

        [Required(ErrorMessage = "Issue number is required.")]
        public string IssueNumber { get; set; } = string.Empty;
        
        [Column(TypeName = "timestamp without time zone")]
        public DateTime IssueDate { get; set; }
        public string? CoverImagePath { get; set; }
        public IssueStatus Status { get; set; } = IssueStatus.InProgress;
        
        // Внешние ключи
        [Range(1, int.MaxValue, ErrorMessage = "You must select a publication.")]
        public int PublicationId { get; set; }
        public Publication Publication { get; set; } = null!;
        
        // Навигационные свойства
        public List<Article> Articles { get; set; } = new();
        public List<PageLayout> PageLayouts { get; set; } = new();
        
        public bool IsFeatured { get; set; } = false;
        public int ViewCount { get; set; } = 0;
        public string? Description { get; set; }
    }
}