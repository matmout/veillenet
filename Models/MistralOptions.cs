namespace VeilleNet.Models;

public class MistralOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.mistral.ai/v1/";
    public string Model { get; set; } = "mistral-small-latest";

    public int CacheMinutes { get; set; } = 360;
    public int MaxInputChars { get; set; } = 12000;
    public int MaxOutputTokens { get; set; } = 1350;
    public float Temperature { get; set; } = 0.2f;
}
