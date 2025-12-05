namespace VeilleNet.Models;

public class NewsletterSubscription
{
    public string Email { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; }
    public List<string> Topics { get; set; } = new();
}
