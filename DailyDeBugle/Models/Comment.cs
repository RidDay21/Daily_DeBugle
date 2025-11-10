namespace DailyDeBugle.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Внешние ключи
        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;
        public int EditorId { get; set; }
        public User Editor { get; set; } = null!;
    }
}