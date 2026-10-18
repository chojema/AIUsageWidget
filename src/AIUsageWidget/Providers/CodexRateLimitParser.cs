using System.Text.Json.Nodes;
using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

/// <summary>
/// Maps a Codex app-server <c>RateLimitSnapshot</c> to the 5-hour and weekly windows.
/// The window meaning comes from <c>windowDurationMins</c>; primary and secondary are never assumed.
/// </summary>
public static class CodexRateLimitParser
{
    private const int FiveHourMinutes = 5 * 60;
    private const int WeekMinutes = 7 * 24 * 60;

    public static (UsageWindow? FiveHour, UsageWindow? Weekly) Parse(JsonNode? snapshot)
    {
        UsageWindow? fiveHour = null;
        UsageWindow? weekly = null;
        if (snapshot is not JsonObject obj)
        {
            return (null, null);
        }

        foreach (string key in new[] { "primary", "secondary" })
        {
            if (obj[key] is not JsonObject window || ParseWindow(window) is not { } parsed)
            {
                continue;
            }

            long? minutes = window["windowDurationMins"] is JsonValue m && m.TryGetValue(out long value) ? value : null;
            switch (Classify(minutes))
            {
                case WindowKind.FiveHour:
                    fiveHour ??= parsed;
                    break;
                case WindowKind.Weekly:
                    weekly ??= parsed;
                    break;
            }
        }

        return (fiveHour, weekly);
    }

    /// <summary>Applies a sparse <c>account/rateLimits/updated</c> snapshot over the last known values.</summary>
    public static (UsageWindow? FiveHour, UsageWindow? Weekly) Merge(
        (UsageWindow? FiveHour, UsageWindow? Weekly) current, JsonNode? update)
    {
        var (fiveHour, weekly) = Parse(update);
        return (fiveHour ?? current.FiveHour, weekly ?? current.Weekly);
    }

    internal enum WindowKind { Unknown, FiveHour, Weekly }

    internal static WindowKind Classify(long? minutes) => minutes switch
    {
        null => WindowKind.Unknown,
        >= FiveHourMinutes - 60 and <= FiveHourMinutes + 60 => WindowKind.FiveHour,
        >= WeekMinutes - 24 * 60 and <= WeekMinutes + 24 * 60 => WindowKind.Weekly,
        _ => WindowKind.Unknown,
    };

    private static UsageWindow? ParseWindow(JsonObject window)
    {
        if (window["usedPercent"] is not JsonValue usedValue || !usedValue.TryGetValue(out double used))
        {
            return null;
        }

        DateTimeOffset? resetsAt = window["resetsAt"] is JsonValue r && r.TryGetValue(out long seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
        return new UsageWindow(used, resetsAt);
    }
}
