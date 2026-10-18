using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

/// <summary>One model group from Antigravity's <c>/usage</c> output, such as "Gemini Models".</summary>
public sealed record AntigravityGroup(string Name, UsageWindow? FiveHour, UsageWindow? Weekly);

public enum AntigravityParseStatus
{
    Ok,
    /// <summary>The output is not the answer of the read-only /usage command (for example an agent turn ran).</summary>
    Unsafe,
    /// <summary>The command ran but reported a failure.</summary>
    Failed,
    /// <summary>The output could not be understood.</summary>
    Malformed,
}

/// <summary>
/// Parses <c>agy -p /usage --output-format json</c>. Per the CLI release notes this answers without
/// starting an agent turn or spending quota; the output is accepted only when it proves exactly that.
/// </summary>
public static class AntigravityUsageParser
{
    public const string DefaultGroup = "Gemini Models";

    public static (AntigravityParseStatus Status, IReadOnlyList<AntigravityGroup> Groups) Parse(string output)
    {
        JsonObject? root = null;
        foreach (string line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                if (JsonNode.Parse(line) is JsonObject obj && obj.ContainsKey("status"))
                {
                    root = obj;
                }
            }
            catch (JsonException)
            {
                // Ignore anything that is not the JSON result line.
            }
        }

        if (root is null)
        {
            return (AntigravityParseStatus.Malformed, []);
        }

        // Guard: a real /usage answer never runs a turn and names the command.
        bool noTurn = root["num_turns"] is JsonValue turns && turns.TryGetValue(out int count) && count == 0;
        bool isUsage = root["command"]?["name"] is JsonValue name && name.TryGetValue(out string? n) && n == "usage";
        if (!noTurn || !isUsage)
        {
            return (AntigravityParseStatus.Unsafe, []);
        }

        if (root["status"] is not JsonValue status || !status.TryGetValue(out string? s) || s != "SUCCESS")
        {
            return (AntigravityParseStatus.Failed, []);
        }

        if (root["command"]?["data"]?["groups"] is not JsonArray groups)
        {
            return (AntigravityParseStatus.Malformed, []);
        }

        var result = new List<AntigravityGroup>();
        foreach (JsonNode? groupNode in groups)
        {
            if (groupNode is not JsonObject group ||
                group["name"] is not JsonValue groupName || !groupName.TryGetValue(out string? title) ||
                string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            UsageWindow? fiveHour = null;
            UsageWindow? weekly = null;
            if (group["buckets"] is JsonArray buckets)
            {
                foreach (JsonNode? bucket in buckets)
                {
                    string? window = bucket?["window"] is JsonValue w && w.TryGetValue(out string? v) ? v : null;
                    UsageWindow? parsed = ParseBucket(bucket as JsonObject);
                    if (parsed is null)
                    {
                        continue;
                    }
                    if (window == "5h")
                    {
                        fiveHour ??= parsed;
                    }
                    else if (window == "weekly")
                    {
                        weekly ??= parsed;
                    }
                }
            }
            result.Add(new AntigravityGroup(title.Trim(), fiveHour, weekly));
        }

        return (AntigravityParseStatus.Ok, result);
    }

    /// <summary>The selected group, else the default group, else the first group.</summary>
    public static AntigravityGroup? Select(IReadOnlyList<AntigravityGroup> groups, string? selected) =>
        groups.FirstOrDefault(g => g.Name == selected)
        ?? groups.FirstOrDefault(g => g.Name == DefaultGroup)
        ?? groups.FirstOrDefault();

    /// <summary>Short form for the panel title: "Gemini Models" becomes "Gemini".</summary>
    public static string ShortName(string groupName)
    {
        string name = groupName.Trim();
        return name.EndsWith(" models", StringComparison.OrdinalIgnoreCase) ? name[..^" models".Length] : name;
    }

    private static UsageWindow? ParseBucket(JsonObject? bucket)
    {
        if (bucket?["remaining_fraction"] is not JsonValue f || !f.TryGetValue(out double fraction))
        {
            return null;
        }

        DateTimeOffset? resetsAt = bucket["reset_time"] is JsonValue r && r.TryGetValue(out string? text) &&
                                   DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeUniversal, out DateTimeOffset time)
            ? time
            : null;
        return new UsageWindow((1.0 - Math.Clamp(fraction, 0.0, 1.0)) * 100.0, resetsAt);
    }
}
