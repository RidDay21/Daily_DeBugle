using DailyDeBugle.Models;

public class GlobalTextStyle
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public Issue Issue { get; set; }
    
    // Font Settings
    public string PrimaryFont { get; set; } = "Times New Roman";
    public string HeadingFont { get; set; } = "Times New Roman";
    
    // Text Sizes
    public int H1Size { get; set; } = 24;
    public int H2Size { get; set; } = 20;
    public int BodySize { get; set; } = 14;
    
    // Line Spacing
    public double BodyLineSpacing { get; set; } = 1.4;
    public double HeadingLineSpacing { get; set; } = 1.2;
    
    // Columns
    public int ColumnCount { get; set; } = 2;
    public double ColumnGap { get; set; } = 1.0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}