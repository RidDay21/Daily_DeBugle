namespace DailyDeBugle.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        
        // Навигационные свойства
        public List<Article> Articles { get; set; } = new();
        public List<Publication> Publications { get; set; } = new();
    }
}