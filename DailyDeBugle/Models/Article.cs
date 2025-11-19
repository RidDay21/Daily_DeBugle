using System.ComponentModel.DataAnnotations;

namespace DailyDeBugle.Models
{
    public class Article
	{
	    public int ArticleId { get; set; }
        
        [Required(ErrorMessage = "Headline is required.")]
    	public string Headline { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Content is required.")]
    	public string Content { get; set; } = string.Empty;
        
    	public DateTime CreatedDate { get; set; } = DateTime.Now;
    	public DateTime ModifiedDate { get; set; } = DateTime.Now;
    	public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    	// Внешние ключи
        [Required(ErrorMessage = "Author must be selected.")]
    	[Range(1, int.MaxValue, ErrorMessage = "Author must be selected.")]
    	public int? AuthorId { get; set; }
    	public User? Author { get; set; }
        
        [Required(ErrorMessage = "Issue must be selected.")]
        [Range(1, int.MaxValue, ErrorMessage = "Issue must be selected.")]
    	public int? IssueId { get; set; }
    	public Issue? Issue { get; set; }
        
    	// Навигационные свойства
    	public List<Comment> Comments { get; set; } = new();
	}
}