using System.Text.Json.Nodes;
using AIUsageWidget.Providers;

namespace AIUsageWidget.Tests;

public class CodexRateLimitParserTests
{
    [Fact]
    public void Maps_windows_by_duration_not_by_position()
    {
        // Secondary is the 5-hour window here; the parser must not assume primary = 5 hours.
        JsonNode snapshot = JsonNode.Parse("""
            {
              "primary":   { "usedPercent": 40, "windowDurationMins": 10080, "resetsAt": 1800000000 },
              "secondary": { "usedPercent": 12, "windowDurationMins": 300,   "resetsAt": 1790000000 }
            }
            """)!;

        var (fiveHour, weekly) = CodexRateLimitParser.Parse(snapshot);

        Assert.Equal(12, fiveHour!.UsedPercent);
        Assert.Equal(88, fiveHour.RemainingPercent);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1790000000), fiveHour.ResetsAt);
        Assert.Equal(40, weekly!.UsedPercent);
    }

    [Fact]
    public void Ignores_windows_with_unknown_duration()
    {
        JsonNode snapshot = JsonNode.Parse("""
            { "primary": { "usedPercent": 5 }, "secondary": { "usedPercent": 9, "windowDurationMins": 60 } }
            """)!;

        var (fiveHour, weekly) = CodexRateLimitParser.Parse(snapshot);

        Assert.Null(fiveHour);
        Assert.Null(weekly);
    }

    [Fact]
    public void Sparse_update_keeps_previous_values()
    {
        var current = CodexRateLimitParser.Parse(JsonNode.Parse("""
            { "primary": { "usedPercent": 10, "windowDurationMins": 300 },
              "secondary": { "usedPercent": 20, "windowDurationMins": 10080 } }
            """));

        var merged = CodexRateLimitParser.Merge(current, JsonNode.Parse("""
            { "primary": { "usedPercent": 15, "windowDurationMins": 300 }, "secondary": null }
            """));

        Assert.Equal(15, merged.FiveHour!.UsedPercent);
        Assert.Equal(20, merged.Weekly!.UsedPercent);
    }

    [Fact]
    public void Null_snapshot_yields_nothing()
    {
        Assert.Equal((null, null), CodexRateLimitParser.Parse(null));
    }
}
