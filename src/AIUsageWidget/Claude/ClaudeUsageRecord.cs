using System.Text.Json;
using System.Text.Json.Nodes;
using AIUsageWidget.Core;

namespace AIUsageWidget.Claude;

/// <summary>
/// The only data the bridge keeps: the two limit windows and when they were seen.
/// Nothing else from the Claude Code session (paths, model, transcript) is stored.
/// </summary>
public sealed record ClaudeUsageRecord(UsageWindow? FiveHour, UsageWindow? Weekly, DateTimeOffset CapturedAt)
{
    /// <summary>Extracts <c>rate_limits</c> from status line input. Returns null when no window is present.</summary>
    public static ClaudeUsageRecord? FromStatusLineInput(string json, DateTimeOffset now)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }

        if (root?["rate_limits"] is not JsonObject limits)
        {
            return null;
        }

        UsageWindow? fiveHour = ParseWindow(limits["five_hour"]);
        UsageWindow? weekly = ParseWindow(limits["seven_day"]);
        return fiveHour is null && weekly is null ? null : new ClaudeUsageRecord(fiveHour, weekly, now);
    }

    private static UsageWindow? ParseWindow(JsonNode? node)
    {
        if (node is not JsonObject window ||
            window["used_percentage"] is not JsonValue usedValue ||
            !usedValue.TryGetValue(out double used))
        {
            return null;
        }

        DateTimeOffset? resetsAt = window["resets_at"] is JsonValue r && r.TryGetValue(out long seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
        return new UsageWindow(used, resetsAt);
    }

    public string ToJson() => new JsonObject
    {
        ["capturedAt"] = CapturedAt.ToUnixTimeSeconds(),
        ["fiveHour"] = WindowToJson(FiveHour),
        ["sevenDay"] = WindowToJson(Weekly),
    }.ToJsonString();

    public static ClaudeUsageRecord? FromJson(string json)
    {
        try
        {
            if (JsonNode.Parse(json) is not JsonObject obj ||
                obj["capturedAt"] is not JsonValue c || !c.TryGetValue(out long captured))
            {
                return null;
            }
            return new ClaudeUsageRecord(
                WindowFromJson(obj["fiveHour"]),
                WindowFromJson(obj["sevenDay"]),
                DateTimeOffset.FromUnixTimeSeconds(captured));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static JsonNode? WindowToJson(UsageWindow? window) => window is null
        ? null
        : new JsonObject
        {
            ["usedPercent"] = window.UsedPercent,
            ["resetsAt"] = window.ResetsAt?.ToUnixTimeSeconds(),
        };

    private static UsageWindow? WindowFromJson(JsonNode? node)
    {
        if (node is not JsonObject obj || obj["usedPercent"] is not JsonValue u || !u.TryGetValue(out double used))
        {
            return null;
        }
        DateTimeOffset? resetsAt = obj["resetsAt"] is JsonValue r && r.TryGetValue(out long s)
            ? DateTimeOffset.FromUnixTimeSeconds(s)
            : null;
        return new UsageWindow(used, resetsAt);
    }
}
