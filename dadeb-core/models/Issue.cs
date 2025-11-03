namespace DefaultNamespace;

public class Issue
{
    public int Id { get; set; }
    public string IssueNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string? CoverImagePath { get; set; }
    public string Status { get; set; } = "In Progress"; // "In Progress", "Layout", "Ready", "Published"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Внешний ключ
    public int PublicationId { get; set; }
    public Publication Publication { get; set; } = null!;
    
    // Навигационные свойства
    public List<Article> Articles { get; set; } = new();
}