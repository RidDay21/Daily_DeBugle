using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyDeBugle.Models
{
    public class Article
    {
        public int ArticleId { get; set; }
        
        [Required(ErrorMessage = "Headline is required.")]
        [MaxLength(200, ErrorMessage = "Headline cannot exceed 200 characters.")]
        public string Headline { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Content is required.")]
        [MinLength(50, ErrorMessage = "Content must be at least 50 characters.")]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

        // Новые свойства для поддержки продолжений
        public int? ContinuedFromArticleId { get; set; }  // ID статьи, от которой это продолжение
        public int? ContinuedOnArticleId { get; set; }    // ID статьи, куда продолжается эта статья
        public int? ContinuedOnPage { get; set; }         // Номер страницы, где продолжение
        public int? StartPage { get; set; }               // Страница, где начинается статья
        public bool HasContinuation { get; set; } = false; // Есть ли продолжение на другой странице
        
        // Статистика для автоматической пагинации
        public int EstimatedHeightCm { get; set; }        // Расчетная высота в сантиметрах
        public int WordCount { get; set; }                // Количество слов
        public int CharacterCount { get; set; }           // Количество символов
        public int ParagraphCount { get; set; }           // Количество абзацев
        
        // Настройки отображения (из типографики)
        public int? FontSize { get; set; }                // Размер шрифта в pt
        public string? FontFamily { get; set; }           // Шрифт
        public double? LineSpacing { get; set; }          // Межстрочный интервал
        public int? ColumnCount { get; set; }             // Количество колонок для этой статьи
        
        // Внешние ключи
        [Required(ErrorMessage = "Author must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "Author must be selected.")]
        public int? AuthorId { get; set; }
        public User? Author { get; set; }
        
        [Required(ErrorMessage = "Issue must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "Issue must be selected.")]
        public int? IssueId { get; set; }
        public Issue? Issue { get; set; }
        
        // Навигационные свойства для продолжений
        [ForeignKey("ContinuedFromArticleId")]
        public Article? ContinuedFromArticle { get; set; }
        
        [ForeignKey("ContinuedOnArticleId")]
        public Article? ContinuedOnArticle { get; set; }
        
        // Навигационные свойства
        public List<Comment> Comments { get; set; } = new();
        public List<LayoutElement> LayoutElements { get; set; } = new();
        public List<ArticlePart> ArticleParts { get; set; } = new();
<<<<<<< HEAD
        public List<ArticleImage> Images { get; set; } = new();
=======
        
        
        //Добавлено для блокировки статьи.
        public int? LockedByUserId { get; set; }
        public User? LockedByUser { get; set; }
        public DateTime? LockedAt { get; set; }

        [NotMapped]
        public bool IsLocked => LockedByUserId.HasValue && LockedAt.HasValue;
>>>>>>> laptev/collaboration
    }

    // Класс для части статьи (если разбита на несколько страниц)
    public class ArticlePart
    {
        public int ArticlePartId { get; set; }
        public int ArticleId { get; set; }
        public string ContentPart { get; set; } = string.Empty;
        public int PartNumber { get; set; }
        public int TotalParts { get; set; }
        public bool IsBeginning { get; set; }
        public bool IsEnding { get; set; }
        public int? PageNumber { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public Article Article { get; set; } = null!;
    }
}