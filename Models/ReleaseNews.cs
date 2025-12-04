namespace VeilleNet.Models;

public class ReleaseNews
{
    public string Version { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Preview, RC, Stable
}
