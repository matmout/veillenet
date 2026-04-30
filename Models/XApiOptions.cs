namespace VeilleNet.Models;

public class XApiOptions
{
    public const string SectionName = "XApi";

    public bool Enabled { get; set; } = false;
    public string BaseUrl { get; set; } = "https://api.x.com/2/";
    public string BearerToken { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public int PostsPerUser { get; set; } = 5;
    public int CacheMinutes { get; set; } = 1440;
}
