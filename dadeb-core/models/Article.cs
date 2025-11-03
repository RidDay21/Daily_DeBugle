namespace DefaultNamespace;

public class Article
{
    public int Id { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // "Draft", "Under Review", "Approved", "Rejected"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Внешний ключ
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
}