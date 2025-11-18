namespace DailyDeBugle.Models
{
    public class LayoutElement
    {
        public int LayoutElementId { get; set; }
        public ElementType Type { get; set; }
        public string Position { get; set; } = string.Empty; // JSON с координатами
        public string Size { get; set; } = string.Empty; // JSON с размерами
        /// <summary>
        /// Добавил вот эту штучку
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Внешние ключи
        public int PageLayoutId { get; set; }
        public PageLayout PageLayout { get; set; } = null!;
        public int? ArticleId { get; set; }
        public Article? Article { get; set; }
        public int? AdvertisementBlockId { get; set; }
        public AdvertisementBlock? AdvertisementBlock { get; set; }
    }
}