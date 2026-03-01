using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeilleNet.Data.SeedData;

/// <summary>
/// Helper to load JSON seed data from embedded resources.
/// </summary>
public static class SeedDataLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Load and deserialize a JSON seed data file embedded as a resource.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="fileName">The JSON file name (e.g. "links.json").</param>
    /// <returns>The deserialized object.</returns>
    public static T Load<T>(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith($".SeedData.{fileName}", StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Embedded resource '{fileName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Could not open embedded resource stream for '{resourceName}'.");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize '{fileName}' to {typeof(T).Name}.");
    }
}
