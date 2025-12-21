namespace DailyDeBugle.Models
{
    public class LayoutElement
    {
        public int LayoutElementId { get; set; }
        public ElementType Type { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string? TextFlow { get; set; } = "None";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public int PageLayoutId { get; set; }
        public PageLayout PageLayout { get; set; } = null!;
        
        // ОДНО поле для Article
        public int? ArticleId { get; set; }
        public Article? Article { get; set; }
        
        // ОДНО поле для AdvertisementBlock
        public int? AdvertisementBlockId { get; set; }
        public AdvertisementBlock? AdvertisementBlock { get; set; }
    }
}