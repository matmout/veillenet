using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using VeilleNet.Data.SeedData;
using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IFrameworkVersionService
{
    Task<List<FrameworkVersion>> GetAllVersionsAsync();
    Task<List<FrameworkVersion>> GetVersionsByFrameworkAsync(string framework);
    List<FrameworkVersion> GetEndingSoonVersions(int monthsThreshold = 6);
    List<string> GetFrameworkNames();
    DateTime GetDataVerifiedAt();
}

/// <summary>
/// Seed data DTO for deserializing framework-versions.json.
/// Uses string-based enums and ISO date strings.
/// </summary>
file sealed class FrameworkVersionSeed
{
    public string Framework { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ReleaseDate { get; set; } = string.Empty;
    public string? EndOfSupportDate { get; set; }
    public string? TimelineEndDate { get; set; }
    public string SupportType { get; set; } = string.Empty;
    public string KeyFeatures { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? MigrationGuideUrl { get; set; }
    public string? SupportNote { get; set; }
}

public class FrameworkVersionService : IFrameworkVersionService
{
    private static readonly DateTime DataVerifiedAt = new(2026, 4, 30);
    private readonly List<FrameworkVersion> _versions;

    public FrameworkVersionService()
    {
        _versions = LoadVersions();
        ComputeStatuses();
    }

    public Task<List<FrameworkVersion>> GetAllVersionsAsync() =>
        Task.FromResult(_versions.ToList());

    public Task<List<FrameworkVersion>> GetVersionsByFrameworkAsync(string framework)
    {
        var filtered = _versions
            .Where(v => v.Framework.Equals(framework, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult(filtered);
    }

    public List<FrameworkVersion> GetEndingSoonVersions(int monthsThreshold = 6)
    {
        var threshold = DateTime.Today.AddMonths(monthsThreshold);
        return _versions
            .Where(v => v.EndOfSupportDate.HasValue
                        && v.EndOfSupportDate.Value > DateTime.Today
                        && v.EndOfSupportDate.Value <= threshold)
            .OrderBy(v => v.EndOfSupportDate)
            .ToList();
    }

    public List<string> GetFrameworkNames() =>
        _versions.Select(v => v.Framework).Distinct().OrderBy(f => f).ToList();

    public DateTime GetDataVerifiedAt() => DataVerifiedAt;

    private void ComputeStatuses()
    {
        var today = DateTime.Today;
        var sixMonths = today.AddMonths(6);

        foreach (var v in _versions)
        {
            if (v.SupportType == SupportType.Preview)
            {
                v.Status = SupportStatus.Preview;
                v.AdoptionLabel = AdoptionLabel.TestOnly;
            }
            else if (v.EndOfSupportDate.HasValue && v.EndOfSupportDate.Value < today)
            {
                v.Status = SupportStatus.EndOfLife;
                v.AdoptionLabel = AdoptionLabel.AvoidForProd;
            }
            else if (v.EndOfSupportDate.HasValue && v.EndOfSupportDate.Value <= sixMonths)
            {
                v.Status = SupportStatus.EndingSoon;
                v.AdoptionLabel = v.SupportType == SupportType.LTS ? AdoptionLabel.Recommended : AdoptionLabel.TestOnly;
            }
            else
            {
                v.Status = SupportStatus.Active;
                if (v.SupportType == SupportType.LTS)
                    v.AdoptionLabel = AdoptionLabel.Recommended;
                else if (v.SupportType == SupportType.Legacy)
                    v.AdoptionLabel = AdoptionLabel.AvoidForProd;
                else
                    v.AdoptionLabel = AdoptionLabel.Recommended;
            }
        }
    }

    private static List<FrameworkVersion> LoadVersions()
    {
        var seeds = SeedDataLoader.Load<List<FrameworkVersionSeed>>("framework-versions.json");
        return seeds.Select(s => new FrameworkVersion
        {
            Framework = s.Framework,
            Version = s.Version,
            DisplayName = s.DisplayName,
            ReleaseDate = ParseRequiredDate(s.ReleaseDate),
            EndOfSupportDate = ParseOptionalDate(s.EndOfSupportDate),
            TimelineEndDate = ParseOptionalDate(s.TimelineEndDate),
            SupportType = Enum.Parse<SupportType>(s.SupportType),
            KeyFeatures = s.KeyFeatures,
            Url = s.Url,
            MigrationGuideUrl = s.MigrationGuideUrl,
            SupportNote = s.SupportNote
        }).ToList();
    }

    private static DateTime ParseRequiredDate(string value) =>
        DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static DateTime? ParseOptionalDate(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
