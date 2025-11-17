namespace DailyDeBugle.Models;

public class HeaderFooterSettings
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public Issue Issue { get; set; }
    
    // Header Configuration
    public bool HeaderEnabled { get; set; } = true;
    public string HeaderLeft { get; set; }
    public string HeaderCenter { get; set; }
    public string HeaderRight { get; set; }
    
    // Footer Configuration  
    public bool FooterEnabled { get; set; } = true;
    public string FooterLeft { get; set; }
    public string FooterCenter { get; set; }
    public string FooterRight { get; set; }
    
    // Formatting
    public string FontFamily { get; set; } = "Times New Roman";
    public int FontSize { get; set; } = 10;
    public string Alignment { get; set; } = "center";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}