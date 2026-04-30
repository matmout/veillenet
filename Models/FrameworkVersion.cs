namespace VeilleNet.Models;

public class FrameworkVersion
{
    public string Framework { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public DateTime? TimelineEndDate { get; set; }
    public SupportType SupportType { get; set; }
    public SupportStatus Status { get; set; }
    public string KeyFeatures { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? MigrationGuideUrl { get; set; }
    public string? SupportNote { get; set; }
    public AdoptionLabel AdoptionLabel { get; set; }
}

public enum SupportType
{
    LTS,
    STS,
    Preview,
    Legacy
}

public enum SupportStatus
{
    Active,
    EndingSoon,
    EndOfLife,
    Preview
}

public enum AdoptionLabel
{
    Recommended,
    TestOnly,
    AvoidForProd
}
