namespace DailyDeBugle.Models
{
    public class Article
    {
        public int ArticleId { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
        
        // Внешние ключи
        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;
        public int? IssueId { get; set; }
        public Issue? Issue { get; set; }
        
        // Навигационные свойства
        public List<Comment> Comments { get; set; } = new();
    }
}