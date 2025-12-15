namespace VeilleNet.Models;

public class AiContentSummary
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public bool AiGenerated { get; set; } = false;
    public DateTime SummaryDate { get; set; } = DateTime.UtcNow;
}
