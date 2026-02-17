using VeilleNet.Models;

namespace VeilleNet.Services;

public interface IFrameworkVersionService
{
    Task<List<FrameworkVersion>> GetAllVersionsAsync();
    Task<List<FrameworkVersion>> GetVersionsByFrameworkAsync(string framework);
    List<FrameworkVersion> GetEndingSoonVersions(int monthsThreshold = 6);
    List<string> GetFrameworkNames();
}

public class FrameworkVersionService : IFrameworkVersionService
{
    private readonly List<FrameworkVersion> _versions;

    public FrameworkVersionService()
    {
        _versions = BuildVersionData();
        ComputeStatuses();
    }

    public Task<List<FrameworkVersion>> GetAllVersionsAsync()
    {
        return Task.FromResult(_versions.ToList());
    }

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

    public List<string> GetFrameworkNames()
    {
        return _versions.Select(v => v.Framework).Distinct().OrderBy(f => f).ToList();
    }

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

    private static List<FrameworkVersion> BuildVersionData()
    {
        return new List<FrameworkVersion>
        {
            // ============================================================
            // .NET Framework (Legacy)
            // ============================================================
            new() { Framework = ".NET Framework", Version = "4.0", DisplayName = ".NET Framework 4.0",
                ReleaseDate = new DateTime(2010, 4, 12), EndOfSupportDate = new DateTime(2016, 1, 12),
                SupportType = SupportType.Legacy, KeyFeatures = "Dynamic language runtime, Parallel extensions, MEF, Code contracts",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework-4", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.5", DisplayName = ".NET Framework 4.5",
                ReleaseDate = new DateTime(2012, 10, 9), EndOfSupportDate = new DateTime(2016, 1, 12),
                SupportType = SupportType.Legacy, KeyFeatures = "Async/await, Task-based I/O, WinRT interop, ZIP compression",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.5.2", DisplayName = ".NET Framework 4.5.2",
                ReleaseDate = new DateTime(2014, 5, 5), EndOfSupportDate = new DateTime(2022, 4, 26),
                SupportType = SupportType.Legacy, KeyFeatures = "High DPI improvements, ASP.NET perf, Activity tracing",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.6", DisplayName = ".NET Framework 4.6",
                ReleaseDate = new DateTime(2015, 7, 29), EndOfSupportDate = new DateTime(2022, 4, 26),
                SupportType = SupportType.Legacy, KeyFeatures = "RyuJIT compiler, Roslyn, SIMD, Async Task-based methods",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.6.2", DisplayName = ".NET Framework 4.6.2",
                ReleaseDate = new DateTime(2016, 8, 2), EndOfSupportDate = new DateTime(2027, 1, 12),
                SupportType = SupportType.Legacy, KeyFeatures = "Long path support, X509 certificates, ClickOnce TLS 1.1/1.2",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.7.2", DisplayName = ".NET Framework 4.7.2",
                ReleaseDate = new DateTime(2018, 4, 30), EndOfSupportDate = null,
                SupportType = SupportType.Legacy, KeyFeatures = "Dependency injection in ASP.NET, SameSite cookies, Cryptography improvements",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.8", DisplayName = ".NET Framework 4.8",
                ReleaseDate = new DateTime(2019, 4, 18), EndOfSupportDate = null,
                SupportType = SupportType.Legacy, KeyFeatures = "JIT improvements, High DPI WinForms, WCF service behavior, ZLib update",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },
            new() { Framework = ".NET Framework", Version = "4.8.1", DisplayName = ".NET Framework 4.8.1",
                ReleaseDate = new DateTime(2022, 8, 9), EndOfSupportDate = null,
                SupportType = SupportType.Legacy, KeyFeatures = "Final .NET Framework release, Windows-only, tied to OS lifecycle",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet-framework/net481", MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/" },

            // ============================================================
            // .NET Core / .NET (Modern)
            // ============================================================
            new() { Framework = ".NET", Version = "Core 1.0", DisplayName = ".NET Core 1.0",
                ReleaseDate = new DateTime(2016, 6, 27), EndOfSupportDate = new DateTime(2019, 6, 27),
                SupportType = SupportType.STS, KeyFeatures = "Cross-platform, Open source, Kestrel, CLI tooling, CoreCLR",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/1.0" },
            new() { Framework = ".NET", Version = "Core 1.1", DisplayName = ".NET Core 1.1",
                ReleaseDate = new DateTime(2016, 11, 16), EndOfSupportDate = new DateTime(2019, 6, 27),
                SupportType = SupportType.STS, KeyFeatures = "Performance improvements, Additional APIs, .NET Standard 1.6",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/1.1" },
            new() { Framework = ".NET", Version = "Core 2.0", DisplayName = ".NET Core 2.0",
                ReleaseDate = new DateTime(2017, 8, 14), EndOfSupportDate = new DateTime(2018, 10, 1),
                SupportType = SupportType.STS, KeyFeatures = ".NET Standard 2.0, Razor Pages, 20k+ APIs added",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/2.0" },
            new() { Framework = ".NET", Version = "Core 2.1", DisplayName = ".NET Core 2.1 LTS",
                ReleaseDate = new DateTime(2018, 5, 30), EndOfSupportDate = new DateTime(2021, 8, 21),
                SupportType = SupportType.LTS, KeyFeatures = "Span<T>, HttpClientFactory, SignalR, Generic host",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/2.1" },
            new() { Framework = ".NET", Version = "Core 3.0", DisplayName = ".NET Core 3.0",
                ReleaseDate = new DateTime(2019, 9, 23), EndOfSupportDate = new DateTime(2020, 3, 3),
                SupportType = SupportType.STS, KeyFeatures = "WPF/WinForms on Core, C# 8, Blazor Server, gRPC, Worker services",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/3.0" },
            new() { Framework = ".NET", Version = "Core 3.1", DisplayName = ".NET Core 3.1 LTS",
                ReleaseDate = new DateTime(2019, 12, 3), EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS, KeyFeatures = "Blazor WebAssembly preview, Partial class support, C# 8 refinements",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/3.1" },
            new() { Framework = ".NET", Version = "5.0", DisplayName = ".NET 5.0 STS",
                ReleaseDate = new DateTime(2020, 11, 10), EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS, KeyFeatures = "Unified platform, C# 9, Records, Top-level statements, Source generators",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/5.0" },
            new()
            {
                Framework = ".NET",
                Version = "6.0",
                DisplayName = ".NET 6.0 LTS",
                ReleaseDate = new DateTime(2021, 11, 8),
                EndOfSupportDate = new DateTime(2024, 11, 12),
                SupportType = SupportType.LTS,
                KeyFeatures = "Minimal APIs, Hot Reload, C# 10, .NET MAUI preview, Arm64 support",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/6.0",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/aspnet/core/migration/50-to-60"
            },
            new()
            {
                Framework = ".NET",
                Version = "7.0",
                DisplayName = ".NET 7.0 STS",
                ReleaseDate = new DateTime(2022, 11, 8),
                EndOfSupportDate = new DateTime(2024, 5, 14),
                SupportType = SupportType.STS,
                KeyFeatures = "Native AOT, Rate Limiting, Output Caching, C# 11, Performance improvements",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/7.0",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/aspnet/core/migration/60-to-70"
            },
            new()
            {
                Framework = ".NET",
                Version = "8.0",
                DisplayName = ".NET 8.0 LTS",
                ReleaseDate = new DateTime(2023, 11, 14),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.LTS,
                KeyFeatures = "Blazor United, Native AOT everywhere, C# 12, Aspire, Identity API endpoints",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/8.0",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/aspnet/core/migration/70-to-80"
            },
            new()
            {
                Framework = ".NET",
                Version = "9.0",
                DisplayName = ".NET 9.0 STS",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "AI building blocks, Tensor support, LINQ improvements, C# 13, HybridCache",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/9.0",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/aspnet/core/migration/80-to-90"
            },
            new()
            {
                Framework = ".NET",
                Version = "10.0",
                DisplayName = ".NET 10.0 LTS",
                ReleaseDate = new DateTime(2025, 11, 11),
                EndOfSupportDate = new DateTime(2028, 11, 14),
                SupportType = SupportType.LTS,
                KeyFeatures = "C# 14, Performance boosts, Extension members, Cloud-native improvements",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/10.0",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/aspnet/core/migration/90-to-100"
            },

            // ============================================================
            // ASP.NET Core
            // ============================================================
            new() { Framework = "ASP.NET Core", Version = "1.0", DisplayName = "ASP.NET Core 1.0",
                ReleaseDate = new DateTime(2016, 6, 27), EndOfSupportDate = new DateTime(2019, 6, 27),
                SupportType = SupportType.STS, KeyFeatures = "Cross-platform web, Kestrel, Middleware pipeline, Tag Helpers",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-1.0" },
            new() { Framework = "ASP.NET Core", Version = "2.0", DisplayName = "ASP.NET Core 2.0",
                ReleaseDate = new DateTime(2017, 8, 14), EndOfSupportDate = new DateTime(2018, 10, 1),
                SupportType = SupportType.STS, KeyFeatures = "Razor Pages, API conventions, IHostedService, Metapackage",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-2.0" },
            new() { Framework = "ASP.NET Core", Version = "2.1", DisplayName = "ASP.NET Core 2.1 LTS",
                ReleaseDate = new DateTime(2018, 5, 30), EndOfSupportDate = new DateTime(2021, 8, 21),
                SupportType = SupportType.LTS, KeyFeatures = "SignalR, Identity UI, HttpClientFactory, HTTPS by default",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-2.1" },
            new() { Framework = "ASP.NET Core", Version = "3.0", DisplayName = "ASP.NET Core 3.0",
                ReleaseDate = new DateTime(2019, 9, 23), EndOfSupportDate = new DateTime(2020, 3, 3),
                SupportType = SupportType.STS, KeyFeatures = "Blazor Server, gRPC, Worker services, Endpoint routing",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-3.0" },
            new() { Framework = "ASP.NET Core", Version = "3.1", DisplayName = "ASP.NET Core 3.1 LTS",
                ReleaseDate = new DateTime(2019, 12, 3), EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS, KeyFeatures = "Blazor WebAssembly preview, Component model, Partial class support",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-3.1" },
            new() { Framework = "ASP.NET Core", Version = "5.0", DisplayName = "ASP.NET Core 5.0 STS",
                ReleaseDate = new DateTime(2020, 11, 10), EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS, KeyFeatures = "Blazor WASM GA, OpenAPI, HTTP/2 gRPC, Model binding improvements",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-5.0" },
            new()
            {
                Framework = "ASP.NET Core",
                Version = "6.0",
                DisplayName = "ASP.NET Core 6.0 LTS",
                ReleaseDate = new DateTime(2021, 11, 8),
                EndOfSupportDate = new DateTime(2024, 11, 12),
                SupportType = SupportType.LTS,
                KeyFeatures = "Minimal APIs, Hot Reload, SignalR improvements, Blazor improvements",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-6.0"
            },
            new()
            {
                Framework = "ASP.NET Core",
                Version = "7.0",
                DisplayName = "ASP.NET Core 7.0 STS",
                ReleaseDate = new DateTime(2022, 11, 8),
                EndOfSupportDate = new DateTime(2024, 5, 14),
                SupportType = SupportType.STS,
                KeyFeatures = "Rate Limiting, Output Caching, minimal API filters, gRPC JSON transcoding",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-7.0"
            },
            new()
            {
                Framework = "ASP.NET Core",
                Version = "8.0",
                DisplayName = "ASP.NET Core 8.0 LTS",
                ReleaseDate = new DateTime(2023, 11, 14),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.LTS,
                KeyFeatures = "Blazor United, Identity API endpoints, Native AOT for Web, Server-side rendering",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-8.0"
            },
            new()
            {
                Framework = "ASP.NET Core",
                Version = "9.0",
                DisplayName = "ASP.NET Core 9.0 STS",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "HybridCache, OpenAPI built-in, SignalR trimming, Static asset delivery",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-9.0"
            },
            new()
            {
                Framework = "ASP.NET Core",
                Version = "10.0",
                DisplayName = "ASP.NET Core 10.0 LTS",
                ReleaseDate = new DateTime(2025, 11, 11),
                EndOfSupportDate = new DateTime(2028, 11, 14),
                SupportType = SupportType.LTS,
                KeyFeatures = "Blazor improvements, Performance enhancements, Cloud-native hosting",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0"
            },

            // ============================================================
            // EF Core
            // ============================================================
            new() { Framework = "EF Core", Version = "1.0", DisplayName = "EF Core 1.0",
                ReleaseDate = new DateTime(2016, 6, 27), EndOfSupportDate = new DateTime(2019, 6, 27),
                SupportType = SupportType.STS, KeyFeatures = "Lightweight ORM, Code-first, Migrations, LINQ provider, Shadow properties",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-1.0" },
            new() { Framework = "EF Core", Version = "2.0", DisplayName = "EF Core 2.0",
                ReleaseDate = new DateTime(2017, 8, 14), EndOfSupportDate = new DateTime(2018, 10, 1),
                SupportType = SupportType.STS, KeyFeatures = "Model-level query filters, Owned entities, Table splitting, DbContext pooling",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-2.0" },
            new() { Framework = "EF Core", Version = "2.1", DisplayName = "EF Core 2.1 LTS",
                ReleaseDate = new DateTime(2018, 5, 30), EndOfSupportDate = new DateTime(2021, 8, 21),
                SupportType = SupportType.LTS, KeyFeatures = "Lazy loading, Value conversions, Query types, Data seeding",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-2.1" },
            new() { Framework = "EF Core", Version = "3.1", DisplayName = "EF Core 3.1 LTS",
                ReleaseDate = new DateTime(2019, 12, 3), EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS, KeyFeatures = "Cosmos DB provider, LINQ improvements, Nullable reference types, C# 8",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.x/whatsnew" },
            new() { Framework = "EF Core", Version = "5.0", DisplayName = "EF Core 5.0 STS",
                ReleaseDate = new DateTime(2020, 11, 10), EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS, KeyFeatures = "Many-to-many, TPT mapping, Filtered includes, Split queries, Event counters",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-5.0/whatsnew" },
            new()
            {
                Framework = "EF Core",
                Version = "6.0",
                DisplayName = "EF Core 6.0 LTS",
                ReleaseDate = new DateTime(2021, 11, 8),
                EndOfSupportDate = new DateTime(2024, 11, 12),
                SupportType = SupportType.LTS,
                KeyFeatures = "Temporal tables, Migration bundles, Pre-convention model config, Compiled models",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/whatsnew"
            },
            new()
            {
                Framework = "EF Core",
                Version = "7.0",
                DisplayName = "EF Core 7.0 STS",
                ReleaseDate = new DateTime(2022, 11, 8),
                EndOfSupportDate = new DateTime(2024, 5, 14),
                SupportType = SupportType.STS,
                KeyFeatures = "Bulk updates, JSON columns, Stored procedure mapping, Table-per-type perf",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew"
            },
            new()
            {
                Framework = "EF Core",
                Version = "8.0",
                DisplayName = "EF Core 8.0 LTS",
                ReleaseDate = new DateTime(2023, 11, 14),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.LTS,
                KeyFeatures = "Complex types, Primitive collections, Raw SQL for unmapped types, Sentinel values",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew"
            },
            new()
            {
                Framework = "EF Core",
                Version = "9.0",
                DisplayName = "EF Core 9.0 STS",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "LINQ improvements, Azure Cosmos DB provider, AOT compilation, HierarchyId",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew"
            },
            new()
            {
                Framework = "EF Core",
                Version = "10.0",
                DisplayName = "EF Core 10.0 LTS",
                ReleaseDate = new DateTime(2025, 11, 11),
                EndOfSupportDate = new DateTime(2028, 11, 14),
                SupportType = SupportType.LTS,
                KeyFeatures = "Performance improvements, Enhanced LINQ translation, Cloud-native features",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10/whatsnew"
            },

            // ============================================================
            // .NET MAUI
            // ============================================================
            new() { Framework = "MAUI", Version = "6", DisplayName = ".NET MAUI 6",
                ReleaseDate = new DateTime(2022, 5, 23), EndOfSupportDate = new DateTime(2023, 5, 8),
                SupportType = SupportType.STS, KeyFeatures = "First GA release, Cross-platform UI, Handlers architecture, Single project",
                Url = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-6" },
            new() { Framework = "MAUI", Version = "7", DisplayName = ".NET MAUI 7",
                ReleaseDate = new DateTime(2022, 11, 8), EndOfSupportDate = new DateTime(2024, 5, 14),
                SupportType = SupportType.STS, KeyFeatures = "Map control, Context menus, Tooltips, Desktop drag-and-drop, Pointer gestures",
                Url = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-7" },
            new()
            {
                Framework = "MAUI",
                Version = "8",
                DisplayName = ".NET MAUI 8",
                ReleaseDate = new DateTime(2023, 11, 14),
                EndOfSupportDate = new DateTime(2025, 5, 14),
                SupportType = SupportType.STS,
                KeyFeatures = "Keyboard accelerators, Desktop improvements, Memory management, Hybrid Blazor",
                Url = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-8"
            },
            new()
            {
                Framework = "MAUI",
                Version = "9",
                DisplayName = ".NET MAUI 9",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 5, 12),
                SupportType = SupportType.STS,
                KeyFeatures = "HybridWebView, Native controls, Titlebar customization, Multi-window improvements",
                Url = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-9"
            },
            new()
            {
                Framework = "MAUI",
                Version = "10",
                DisplayName = ".NET MAUI 10",
                ReleaseDate = new DateTime(2025, 11, 11),
                EndOfSupportDate = new DateTime(2027, 5, 11),
                SupportType = SupportType.STS,
                KeyFeatures = "Performance improvements, Enhanced platform integration, New controls",
                Url = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10"
            },

            // ============================================================
            // C# Language
            // ============================================================
            new() { Framework = "C#", Version = "4", DisplayName = "C# 4",
                ReleaseDate = new DateTime(2010, 4, 12), EndOfSupportDate = new DateTime(2016, 1, 12),
                SupportType = SupportType.LTS, KeyFeatures = "Dynamic binding, Named/optional arguments, Covariance/contravariance",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-40" },
            new() { Framework = "C#", Version = "5", DisplayName = "C# 5",
                ReleaseDate = new DateTime(2012, 8, 15), EndOfSupportDate = new DateTime(2016, 1, 12),
                SupportType = SupportType.LTS, KeyFeatures = "Async/await, Caller info attributes",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-50" },
            new() { Framework = "C#", Version = "6", DisplayName = "C# 6",
                ReleaseDate = new DateTime(2015, 7, 20), EndOfSupportDate = new DateTime(2022, 4, 26),
                SupportType = SupportType.LTS, KeyFeatures = "Null-conditional operator, String interpolation, Expression-bodied members, nameof",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-6" },
            new() { Framework = "C#", Version = "7", DisplayName = "C# 7",
                ReleaseDate = new DateTime(2017, 3, 7), EndOfSupportDate = new DateTime(2022, 4, 26),
                SupportType = SupportType.LTS, KeyFeatures = "Pattern matching, Tuples, Local functions, Out variables, Ref returns",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-7" },
            new() { Framework = "C#", Version = "8", DisplayName = "C# 8",
                ReleaseDate = new DateTime(2019, 9, 23), EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS, KeyFeatures = "Nullable reference types, Async streams, Switch expressions, Default interface methods",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8" },
            new() { Framework = "C#", Version = "9", DisplayName = "C# 9",
                ReleaseDate = new DateTime(2020, 11, 10), EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS, KeyFeatures = "Records, Top-level statements, Init-only setters, Pattern matching enhancements",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9" },
            new()
            {
                Framework = "C#",
                Version = "10",
                DisplayName = "C# 10",
                ReleaseDate = new DateTime(2021, 11, 8),
                EndOfSupportDate = new DateTime(2024, 11, 12), // Tied to .NET 6
                SupportType = SupportType.LTS,
                KeyFeatures = "Global usings, File-scoped namespaces, Record structs, Interpolated string handlers",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10"
            },
            new()
            {
                Framework = "C#",
                Version = "11",
                DisplayName = "C# 11",
                ReleaseDate = new DateTime(2022, 11, 8),
                EndOfSupportDate = new DateTime(2024, 5, 14), // Tied to .NET 7
                SupportType = SupportType.STS,
                KeyFeatures = "Raw string literals, Generic math, List patterns, Required members, UTF-8 strings",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11"
            },
            new()
            {
                Framework = "C#",
                Version = "12",
                DisplayName = "C# 12",
                ReleaseDate = new DateTime(2023, 11, 14),
                EndOfSupportDate = new DateTime(2026, 11, 10), // Tied to .NET 8
                SupportType = SupportType.LTS,
                KeyFeatures = "Primary constructors, Collection expressions, Alias any type, Default lambda parameters",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12"
            },
            new()
            {
                Framework = "C#",
                Version = "13",
                DisplayName = "C# 13",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 11, 10), // Tied to .NET 9
                SupportType = SupportType.STS,
                KeyFeatures = "params collections, New lock type, New escape sequence, Implicit indexer access",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13"
            },
            new()
            {
                Framework = "C#",
                Version = "14",
                DisplayName = "C# 14",
                ReleaseDate = new DateTime(2025, 11, 18),
                EndOfSupportDate = new DateTime(2028, 11, 14), // Tied to .NET 10
                SupportType = SupportType.LTS,
                KeyFeatures = "Extension members, field keyword, Unbound generic types in nameof, First-class spans",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14"
            },

            // ============================================================
            // Visual Studio
            // ============================================================
            new() { Framework = "Visual Studio", Version = "2010", DisplayName = "Visual Studio 2010",
                ReleaseDate = new DateTime(2010, 6, 29), EndOfSupportDate = new DateTime(2020, 7, 14),
                SupportType = SupportType.LTS, KeyFeatures = "WPF editor, Multi-monitor, IntelliTrace, C# 4, .NET 4",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/visual-studio-2010" },
            new() { Framework = "Visual Studio", Version = "2012", DisplayName = "Visual Studio 2012",
                ReleaseDate = new DateTime(2012, 10, 31), EndOfSupportDate = new DateTime(2023, 1, 10),
                SupportType = SupportType.LTS, KeyFeatures = "Unit test explorer, Code review, Page inspector, C# 5",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/visual-studio-2012" },
            new() { Framework = "Visual Studio", Version = "2013", DisplayName = "Visual Studio 2013",
                ReleaseDate = new DateTime(2013, 10, 17), EndOfSupportDate = new DateTime(2024, 4, 9),
                SupportType = SupportType.LTS, KeyFeatures = "CodeLens, Connected Services, Browser Link, Git integration",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/visual-studio-2013" },
            new() { Framework = "Visual Studio", Version = "2015", DisplayName = "Visual Studio 2015",
                ReleaseDate = new DateTime(2015, 7, 20), EndOfSupportDate = new DateTime(2025, 10, 14),
                SupportType = SupportType.LTS, KeyFeatures = "Roslyn compiler, C# 6, Xamarin integration, Universal Windows apps",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/visual-studio-2015" },
            new() { Framework = "Visual Studio", Version = "2017", DisplayName = "Visual Studio 2017",
                ReleaseDate = new DateTime(2017, 3, 7), EndOfSupportDate = new DateTime(2027, 4, 13),
                SupportType = SupportType.LTS, KeyFeatures = "Lightweight install, Live unit testing, C# 7, .NET Core tooling",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/visual-studio-2017" },
            new() { Framework = "Visual Studio", Version = "2019", DisplayName = "Visual Studio 2019",
                ReleaseDate = new DateTime(2019, 4, 2), EndOfSupportDate = new DateTime(2029, 4, 10),
                SupportType = SupportType.LTS, KeyFeatures = "AI-assisted IntelliCode, Live Share, C# 8/9, Git-first workflow",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/visual-studio-2019" },
            new()
            {
                Framework = "Visual Studio",
                Version = "2022",
                DisplayName = "Visual Studio 2022",
                ReleaseDate = new DateTime(2021, 11, 8),
                EndOfSupportDate = null, // Serviced via Channel updates
                SupportType = SupportType.LTS,
                KeyFeatures = "64-bit IDE, IntelliCode, Live Share, Hot Reload, Git integration, Copilot",
                Url = "https://visualstudio.microsoft.com/vs/"
            },
            new()
            {
                Framework = "Visual Studio",
                Version = "2026",
                DisplayName = "Visual Studio 2026",
                ReleaseDate = new DateTime(2025, 12, 20),
                EndOfSupportDate = null,
                SupportType = SupportType.LTS,
                KeyFeatures = "AI-first IDE, Copilot Workspace, Agentic Coding, Performance overhaul",
                Url = "https://visualstudio.microsoft.com/"
            },

            // ============================================================
            // Blazor (follows ASP.NET Core lifecycle)
            // ============================================================
            new() { Framework = "Blazor", Version = "3.1", DisplayName = "Blazor 3.1",
                ReleaseDate = new DateTime(2019, 12, 3), EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS, KeyFeatures = "First GA Blazor Server, WebAssembly preview, Component model",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/" },
            new() { Framework = "Blazor", Version = "5.0", DisplayName = "Blazor 5.0 STS",
                ReleaseDate = new DateTime(2020, 11, 10), EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS, KeyFeatures = "Blazor WebAssembly GA, CSS isolation, JavaScript isolation, Lazy loading",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/" },
            new() { Framework = "Blazor", Version = "6.0", DisplayName = "Blazor 6.0 LTS",
                ReleaseDate = new DateTime(2021, 11, 8), EndOfSupportDate = new DateTime(2024, 11, 12),
                SupportType = SupportType.LTS, KeyFeatures = "Hot Reload, Error boundaries, Dynamic components, Preserve prerendered state",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/" },
            new() { Framework = "Blazor", Version = "7.0", DisplayName = "Blazor 7.0 STS",
                ReleaseDate = new DateTime(2022, 11, 8), EndOfSupportDate = new DateTime(2024, 5, 14),
                SupportType = SupportType.STS, KeyFeatures = "Custom elements, Data binding improvements, Empty content, Virtualization enhancements",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/" },
            new()
            {
                Framework = "Blazor",
                Version = "8.0",
                DisplayName = "Blazor 8.0 LTS",
                ReleaseDate = new DateTime(2023, 11, 14),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.LTS,
                KeyFeatures = "Blazor United (SSR + Interactive), Stream rendering, Enhanced forms, Sections",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/"
            },
            new()
            {
                Framework = "Blazor",
                Version = "9.0",
                DisplayName = "Blazor 9.0 STS",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Static SSR improvements, Reconnection UX, Component rendering optimizations",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/"
            },
            new()
            {
                Framework = "Blazor",
                Version = "10.0",
                DisplayName = "Blazor 10.0 LTS",
                ReleaseDate = new DateTime(2025, 11, 11),
                EndOfSupportDate = new DateTime(2028, 11, 14),
                SupportType = SupportType.LTS,
                KeyFeatures = "Enhanced interactivity, Performance improvements, New component features",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/"
            },

            // ============================================================
            // .NET Aspire
            // ============================================================
            new()
            {
                Framework = ".NET Aspire",
                Version = "8.0",
                DisplayName = ".NET Aspire 8.0",
                ReleaseDate = new DateTime(2024, 5, 21),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Cloud-native app orchestration, Dashboard, Service discovery, Health checks",
                Url = "https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview"
            },
            new()
            {
                Framework = ".NET Aspire",
                Version = "9.0",
                DisplayName = ".NET Aspire 9.0",
                ReleaseDate = new DateTime(2024, 11, 12),
                EndOfSupportDate = new DateTime(2026, 11, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Improved dashboard, Better container support, Enhanced integration components",
                Url = "https://learn.microsoft.com/en-us/dotnet/aspire/"
            },

            // ============================================================
            // Windows (Client)
            // ============================================================
            new() { Framework = "Windows", Version = "7", DisplayName = "Windows 7",
                ReleaseDate = new DateTime(2009, 10, 22), EndOfSupportDate = new DateTime(2020, 1, 14),
                SupportType = SupportType.LTS, KeyFeatures = "Taskbar redesign, Aero Snap, Libraries, HomeGroup, DirectX 11",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-7" },
            new() { Framework = "Windows", Version = "8.1", DisplayName = "Windows 8.1",
                ReleaseDate = new DateTime(2013, 10, 17), EndOfSupportDate = new DateTime(2023, 1, 10),
                SupportType = SupportType.LTS, KeyFeatures = "Start button return, SkyDrive integration, Boot to desktop, Modern UI apps",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-81" },
            new()
            {
                Framework = "Windows",
                Version = "10",
                DisplayName = "Windows 10",
                ReleaseDate = new DateTime(2015, 7, 29),
                EndOfSupportDate = new DateTime(2025, 10, 14),
                SupportType = SupportType.LTS,
                KeyFeatures = "UWP apps, DirectX 12, Virtual desktops, WSL, Windows Hello, Cortana",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-10-home-and-pro"
            },
            new()
            {
                Framework = "Windows",
                Version = "11",
                DisplayName = "Windows 11",
                ReleaseDate = new DateTime(2021, 10, 5),
                EndOfSupportDate = null, // Active support, updated yearly
                SupportType = SupportType.LTS,
                KeyFeatures = "Snap Layouts, Widgets, Android apps, DirectStorage, TPM 2.0, Copilot",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-11-home-and-pro"
            },

            // ============================================================
            // Windows Server
            // ============================================================
            new() { Framework = "Windows Server", Version = "2012", DisplayName = "Windows Server 2012",
                ReleaseDate = new DateTime(2012, 10, 30), EndOfSupportDate = new DateTime(2023, 10, 10),
                SupportType = SupportType.LTS, KeyFeatures = "Hyper-V 3.0, ReFS, Storage Spaces, PowerShell 3.0, SMB 3.0",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2012" },
            new() { Framework = "Windows Server", Version = "2012 R2", DisplayName = "Windows Server 2012 R2",
                ReleaseDate = new DateTime(2013, 10, 18), EndOfSupportDate = new DateTime(2023, 10, 10),
                SupportType = SupportType.LTS, KeyFeatures = "Work Folders, Desired State Config, Storage tiering, Generation 2 VMs",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2012-r2" },
            new() { Framework = "Windows Server", Version = "2016", DisplayName = "Windows Server 2016",
                ReleaseDate = new DateTime(2016, 10, 12), EndOfSupportDate = new DateTime(2027, 1, 12),
                SupportType = SupportType.LTS, KeyFeatures = "Nano Server, Windows Containers, Shielded VMs, Storage Replica",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2016" },
            new()
            {
                Framework = "Windows Server",
                Version = "2019",
                DisplayName = "Windows Server 2019",
                ReleaseDate = new DateTime(2018, 11, 13),
                EndOfSupportDate = new DateTime(2029, 1, 9),
                SupportType = SupportType.LTS,
                KeyFeatures = "Kubernetes support, Windows Admin Center, Storage Migration Service, HCI",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2019"
            },
            new()
            {
                Framework = "Windows Server",
                Version = "2022",
                DisplayName = "Windows Server 2022",
                ReleaseDate = new DateTime(2021, 8, 18),
                EndOfSupportDate = new DateTime(2031, 10, 14),
                SupportType = SupportType.LTS,
                KeyFeatures = "Secured-core server, Azure hybrid, SMB compression, TLS 1.3, Containers",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2022"
            },
            new()
            {
                Framework = "Windows Server",
                Version = "2025",
                DisplayName = "Windows Server 2025",
                ReleaseDate = new DateTime(2024, 11, 1),
                EndOfSupportDate = new DateTime(2034, 10, 10),
                SupportType = SupportType.LTS,
                KeyFeatures = "Hot patching, GPU partitioning, Active Directory improvements, NVMe support",
                Url = "https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2025"
            },
            new()
            {
                Framework = ".NET Framework",
                Version = "4.7.2",
                DisplayName = ".NET Framework 4.7.2",
                ReleaseDate = new DateTime(2018, 4, 30),
                EndOfSupportDate = null,
                SupportType = SupportType.Legacy,
                KeyFeatures = "High DPI improvements, Accessibility enhancements, TLS 1.2 default",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/"
            },
            new()
            {
                Framework = ".NET Framework",
                Version = "4.6.2",
                DisplayName = ".NET Framework 4.6.2",
                ReleaseDate = new DateTime(2016, 8, 2),
                EndOfSupportDate = null,
                SupportType = SupportType.Legacy,
                KeyFeatures = "Long-term stability baseline, TLS improvements, WPF reliability",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/"
            },
            
            // ============================================================
            // .NET (pre-6 modern era)
            // ============================================================
            new()
            {
                Framework = ".NET",
                Version = "5.0",
                DisplayName = ".NET 5.0 STS",
                ReleaseDate = new DateTime(2020, 11, 10),
                EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Unified .NET platform, C# 9, single-file apps, performance improvements",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/5.0",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/"
            },
            new()
            {
                Framework = ".NET",
                Version = "3.1",
                DisplayName = ".NET Core 3.1 LTS",
                ReleaseDate = new DateTime(2019, 12, 3),
                EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS,
                KeyFeatures = "Long-term support, WPF/WinForms on Core, high performance web stack",
                Url = "https://dotnet.microsoft.com/en-us/download/dotnet/3.1",
                MigrationGuideUrl = "https://learn.microsoft.com/en-us/dotnet/core/porting/"
            },
            
            // ============================================================
            // ASP.NET Core (pre-6)
            // ============================================================
            new()
            {
                Framework = "ASP.NET Core",
                Version = "5.0",
                DisplayName = "ASP.NET Core 5.0",
                ReleaseDate = new DateTime(2020, 11, 10),
                EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Improved performance, minimal hosting model foundations, OpenAPI tooling",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-5.0"
            },
            new()
            {
                Framework = "ASP.NET Core",
                Version = "3.1",
                DisplayName = "ASP.NET Core 3.1 LTS",
                ReleaseDate = new DateTime(2019, 12, 3),
                EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS,
                KeyFeatures = "Endpoint routing, gRPC support, high-performance Kestrel",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-3.1"
            },
            
            // ============================================================
            // EF Core (pre-6)
            // ============================================================
            new()
            {
                Framework = "EF Core",
                Version = "5.0",
                DisplayName = "EF Core 5.0",
                ReleaseDate = new DateTime(2020, 11, 10),
                EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Filtered include, many-to-many relationships, split queries",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-5.0/whatsnew"
            },
            new()
            {
                Framework = "EF Core",
                Version = "3.1",
                DisplayName = "EF Core 3.1 LTS",
                ReleaseDate = new DateTime(2019, 12, 3),
                EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS,
                KeyFeatures = "LINQ translation overhaul, improved SQL generation, stability",
                Url = "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.1/"
            },
            
            // ============================================================
            // C# (pre-10)
            // ============================================================
            new()
            {
                Framework = "C#",
                Version = "9",
                DisplayName = "C# 9",
                ReleaseDate = new DateTime(2020, 11, 10),
                EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "Records, init-only setters, pattern matching enhancements, top-level programs",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-9"
            },
            new()
            {
                Framework = "C#",
                Version = "8",
                DisplayName = "C# 8",
                ReleaseDate = new DateTime(2019, 9, 23),
                EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS,
                KeyFeatures = "Nullable reference types, async streams, ranges and indices, switch expressions",
                Url = "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8"
            },
            
            // ============================================================
            // Blazor (pre-8)
            // ============================================================
            new()
            {
                Framework = "Blazor",
                Version = "5.0",
                DisplayName = "Blazor 5.0",
                ReleaseDate = new DateTime(2020, 11, 10),
                EndOfSupportDate = new DateTime(2022, 5, 10),
                SupportType = SupportType.STS,
                KeyFeatures = "WebAssembly GA improvements, component performance, JS interop updates",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/"
            },
            new()
            {
                Framework = "Blazor",
                Version = "3.2",
                DisplayName = "Blazor WebAssembly 3.2",
                ReleaseDate = new DateTime(2020, 5, 19),
                EndOfSupportDate = new DateTime(2022, 12, 13),
                SupportType = SupportType.LTS,
                KeyFeatures = "First supported WebAssembly release, client-side C# in browser",
                Url = "https://learn.microsoft.com/en-us/aspnet/core/blazor/"
            },
            
            // ============================================================
            // Visual Studio (older major)
            // ============================================================
            new()
            {
                Framework = "Visual Studio",
                Version = "2019",
                DisplayName = "Visual Studio 2019",
                ReleaseDate = new DateTime(2019, 4, 2),
                EndOfSupportDate = new DateTime(2029, 4, 10),
                SupportType = SupportType.LTS,
                KeyFeatures = "Improved performance, IntelliCode, Live Share, integrated Git",
                Url = "https://visualstudio.microsoft.com/vs/older-downloads/"
            }
        };
    }
}
