using System.ComponentModel.DataAnnotations;

namespace DailyDeBugle.Models
{
    public class AdvertisementBlock
    {
        public int AdvertisementBlockId { get; set; }
        
        [Required(ErrorMessage = "Please specify the advertiser")]
        [StringLength(100, ErrorMessage = "The name must not exceed 100 characters")]
        public string Advertiser { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Please fill in the advertisement content")]
        [StringLength(1000, ErrorMessage = "The content must not exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Range(1, 10, ErrorMessage = "The width must be between 1 and 10 cm")]
        public double DefaultWidth { get; set; } = 5.0;
        
        [Range(1, 10, ErrorMessage = "The height must be between 1 and 10 cm")]
        public double DefaultHeight { get; set; } = 3.0;
        
        public AdType Type { get; set; } = AdType.Text;
        
        // Navigation properties
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
        
        // Position in pixels for preview
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        
        // Text wrap settings
        public string TextFlow { get; set; } = "None"; // "Left", "Right", "None"
        public double Margin { get; set; } = 0.3;
    }
}