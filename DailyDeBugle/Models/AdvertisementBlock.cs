using System.ComponentModel.DataAnnotations;

namespace DailyDeBugle.Models
{
    public class AdvertisementBlock
    {
        public int AdvertisementBlockId { get; set; }
        
        [Required(ErrorMessage = "Укажите рекламодателя")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Advertiser { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Заполните содержание рекламы")]
        [StringLength(1000, ErrorMessage = "Содержание не должно превышать 1000 символов")]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Range(1, 10, ErrorMessage = "Ширина должна быть от 1 до 10 см")]
        public double DefaultWidth { get; set; } = 5.0;
        
        [Range(1, 10, ErrorMessage = "Высота должна быть от 1 до 10 см")]
        public double DefaultHeight { get; set; } = 3.0;
        
        public AdType Type { get; set; } = AdType.Text;
        
        // Навигационные свойства
        public List<LayoutElement> LayoutElements { get; set; } = new();
    }

    public enum AdType
    {
        Text = 1,
        Image = 2,
        Mixed = 3
    }

    public class AdvertisementPlacementDto
    {
        public int LayoutElementId { get; set; }
        public AdvertisementBlock AdvertisementBlock { get; set; } = null!;
        public int PageNumber { get; set; } = 1;
        
        // Позиция в пикселях для превью
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        
        // Настройки обтекания
        public string TextFlow { get; set; } = "None"; // "Left", "Right", "None"
        public double Margin { get; set; } = 0.3;
    }
}