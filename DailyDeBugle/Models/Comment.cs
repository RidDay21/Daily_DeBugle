namespace DailyDeBugle.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    
        // Внешние ключи
        public int ArticleId { get; set; }
        public Article Article { get; set; } = null!;
    
        // Новое свойство для пометки комментариев редактора
        public bool IsEditorComment { get; set; } = false;
    }
}