namespace VeilleNet.Models;

public class GitHubProject
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int Stars { get; set; }
    public int Forks { get; set; }
    public string Language { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
