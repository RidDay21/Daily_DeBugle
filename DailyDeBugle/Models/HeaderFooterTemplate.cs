namespace DailyDeBugle.Models;

public class HeaderFooterTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string TemplateType { get; set; } // "header" or "footer"
    public string ContentTemplate { get; set; }
    public bool IsSystemTemplate { get; set; } = false;
}