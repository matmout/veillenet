namespace VeilleNet.Models;

public class LLM
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime DateRelease { get; set; }
    public string ScoreIA { get; set; } = string.Empty;
}
