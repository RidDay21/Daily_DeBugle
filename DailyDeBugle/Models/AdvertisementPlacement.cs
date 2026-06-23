// AdvertisementPlacement.cs
namespace DailyDeBugle.Models
{
    public class AdvertisementPlacement
    {
        public int AdvertisementPlacementId { get; set; }
        public int PageNumber { get; set; } = 1;
        
        // Позиция в мм (для печати)
        public double PositionX { get; set; } = 0;
        public double PositionY { get; set; } = 0;
        public double Width { get; set; } = 50; // мм
        public double Height { get; set; } = 30; // мм
        
        // Настройки обтекания текстом
        public string TextFlow { get; set; } = "None"; // Left, Right, None
        public double MarginAround { get; set; } = 3; // мм отступа вокруг
        
        // Внешние ключи
        public int IssueId { get; set; }
        public Issue Issue { get; set; } = null!;
        
        public int AdvertisementBlockId { get; set; }
        public AdvertisementBlock AdvertisementBlock { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Метод для получения CSS стиля
        public string GetStyle(double scale = 1.0)
        {
            return $"left: {PositionX * scale}px; top: {PositionY * scale}px; " +
                   $"width: {Width * scale}px; height: {Height * scale}px;";
        }
    }
}