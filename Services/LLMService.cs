using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using VeilleNet.Data.SeedData;
using VeilleNet.Models;
using VeilleNet.Services.Tools;

namespace VeilleNet.Services;

public interface ILLMService
{
    Task<List<LLM>> GetLatestLLMsAsync();
    Task<List<LLM>> GetTopLLMsAsync(int count);
}

/// <summary>
/// Custom converter to deserialize ISO date strings (YYYY-MM-DD) into DateTime.
/// </summary>
file sealed class DateOnlyConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return DateTime.ParseExact(str!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
}

public class LLMService : ILLMService
{
    private readonly ICacheService _cacheService;
    private const string CacheKey = "LatestLLMs";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    private static readonly Lazy<List<LLM>> _seedData = new(() =>
    {
        var llms = SeedDataLoader.Load<List<LLM>>("llms.json");
        return llms.OrderByDescending(l => l.DateRelease).ToList();
    });

    public LLMService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<List<LLM>> GetLatestLLMsAsync()
    {
        var cachedLLMs = _cacheService.Get<List<LLM>>(CacheKey);
        if (cachedLLMs != null)
        {
            return cachedLLMs;
        }

        var llms = _seedData.Value;
        _cacheService.Set(CacheKey, llms, CacheExpiration);
        return await Task.FromResult(llms);
    }

    public async Task<List<LLM>> GetTopLLMsAsync(int count)
    {
        var allLLMs = await GetLatestLLMsAsync();
        return allLLMs.Take(count).ToList();
    }
}
