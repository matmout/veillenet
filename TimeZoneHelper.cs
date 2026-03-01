using System.Runtime.InteropServices;

namespace VeilleNet;

/// <summary>
/// Cross-platform helper for timezone operations.
/// Windows uses "Romance Standard Time" while Linux/macOS use "Europe/Paris" (IANA).
/// </summary>
public static class TimeZoneHelper
{
    private static readonly Lazy<TimeZoneInfo> ParisTimeZone = new(() =>
    {
        // Try IANA ID first (works on Linux, macOS, and .NET 6+ on Windows)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback to Windows TZID
        }

        // Try Windows TZID (works on Windows)
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            // Last resort: use UTC+1 as a static offset (no DST)
        }

        return TimeZoneInfo.CreateCustomTimeZone("Europe/Paris-Fallback", TimeSpan.FromHours(1),
            "Central European Time", "Central European Standard Time");
    });

    /// <summary>
    /// Gets the Paris (Europe/Paris) timezone — works on both Windows and Linux.
    /// </summary>
    public static TimeZoneInfo GetParisTimeZone() => ParisTimeZone.Value;

    /// <summary>
    /// Converts a UTC DateTime to Paris local time.
    /// </summary>
    public static DateTime ConvertUtcToParis(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
            GetParisTimeZone());
    }

    /// <summary>
    /// Converts a Paris local DateTime to UTC.
    /// </summary>
    public static DateTime ConvertParisToUtc(DateTime parisDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(parisDateTime, GetParisTimeZone());
    }
}
