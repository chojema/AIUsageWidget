using System.Text.Json.Serialization;

namespace AIUsageWidget.Core;

/// <summary>A section of the panel that can be shown, hidden and reordered.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SectionId>))]
public enum SectionId
{
    Codex,
    ClaudeCode,
    Antigravity,
    Date,
    Weekday,
    Time,
    Ime,
}

public sealed class SectionSetting
{
    public SectionId Id { get; set; }

    public bool Visible { get; set; } = true;
}

public static class SectionLayout
{
    /// <summary>The order used before the user changes anything.</summary>
    public static IReadOnlyList<SectionId> DefaultOrder { get; } =
    [
        SectionId.Codex, SectionId.ClaudeCode, SectionId.Antigravity,
        SectionId.Date, SectionId.Weekday, SectionId.Time, SectionId.Ime,
    ];

    public static bool IsProvider(SectionId id) =>
        id is SectionId.Codex or SectionId.ClaudeCode or SectionId.Antigravity;

    public static ProviderId? ProviderOf(SectionId id) => id switch
    {
        SectionId.Codex => ProviderId.Codex,
        SectionId.ClaudeCode => ProviderId.ClaudeCode,
        SectionId.Antigravity => ProviderId.Antigravity,
        _ => null,
    };

    /// <summary>
    /// Returns a complete list with every section exactly once. Saved order and visibility are kept;
    /// sections missing from an older settings file are inserted at their default position.
    /// <paramref name="legacyShowIme"/> carries over the IME on/off setting from versions before sections existed.
    /// </summary>
    public static List<SectionSetting> Normalize(IEnumerable<SectionSetting>? saved, bool legacyShowIme = true)
    {
        var result = new List<SectionSetting>();
        var seen = new HashSet<SectionId>();
        foreach (SectionSetting item in saved ?? [])
        {
            if (Enum.IsDefined(item.Id) && seen.Add(item.Id))
            {
                result.Add(new SectionSetting { Id = item.Id, Visible = item.Visible });
            }
        }

        bool fresh = result.Count == 0;
        for (int i = 0; i < DefaultOrder.Count; i++)
        {
            SectionId id = DefaultOrder[i];
            if (seen.Contains(id))
            {
                continue;
            }
            bool visible = id != SectionId.Ime || !fresh || legacyShowIme;
            // Place it right after the nearest section that precedes it in the default order.
            int after = -1;
            for (int j = i - 1; j >= 0 && after < 0; j--)
            {
                after = result.FindIndex(s => s.Id == DefaultOrder[j]);
            }
            result.Insert(after + 1, new SectionSetting { Id = id, Visible = visible });
            seen.Add(id);
        }
        return result;
    }

    /// <summary>Moves a section one step up (-1) or down (+1). Returns false at either end.</summary>
    public static bool Move(List<SectionSetting> sections, SectionId id, int delta)
    {
        int index = sections.FindIndex(s => s.Id == id);
        int target = index + delta;
        if (index < 0 || target < 0 || target >= sections.Count)
        {
            return false;
        }
        (sections[index], sections[target]) = (sections[target], sections[index]);
        return true;
    }

    public static IReadOnlyList<SectionId> Visible(IEnumerable<SectionSetting> sections) =>
        sections.Where(s => s.Visible).Select(s => s.Id).ToList();
}
