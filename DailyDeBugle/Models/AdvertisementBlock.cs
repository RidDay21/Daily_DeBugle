namespace DailyDeBugle.Models
{
    public class AdvertisementBlock
    {
        public int AdvertisementBlockId { get; set; }
        public string Advertiser { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // Текст или путь к изображению
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Навигационные свойства
        public List<LayoutElement> LayoutElements { get; set; } = new();
    }
}