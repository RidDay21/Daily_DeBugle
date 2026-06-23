using System.ComponentModel.DataAnnotations;

namespace DailyDeBugle.Models;

public class ArticleImage
{
    public int ArticleImageId { get; set; }

    [Required]
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty; // e.g. /uploads/articles/123/....

    [MaxLength(200)]
    public string? Caption { get; set; }

    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

