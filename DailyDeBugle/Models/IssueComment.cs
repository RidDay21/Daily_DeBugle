using System.ComponentModel.DataAnnotations;

namespace DailyDeBugle.Models
{
    public class IssueComment
    {
        public int IssueCommentId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int IssueId { get; set; }
        public Issue Issue { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
