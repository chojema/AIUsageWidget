namespace AIUsageWidget.Core;

public static class RefreshInterval
{
    public const int MinCustomMinutes = 1;
    public const int MaxCustomMinutes = 24 * 60;

    /// <summary>The time between automatic queries, or null for manual refresh only.</summary>
    public static TimeSpan? For(RefreshPreset preset, int customMinutes) => preset switch
    {
        RefreshPreset.Manual => null,
        RefreshPreset.OneMinute => TimeSpan.FromMinutes(1),
        RefreshPreset.FiveMinutes => TimeSpan.FromMinutes(5),
        RefreshPreset.ThirtyMinutes => TimeSpan.FromMinutes(30),
        RefreshPreset.OneHour => TimeSpan.FromHours(1),
        RefreshPreset.Custom => TimeSpan.FromMinutes(ClampCustom(customMinutes)),
        _ => TimeSpan.FromMinutes(5),
    };

    public static int ClampCustom(int minutes) => Math.Clamp(minutes, MinCustomMinutes, MaxCustomMinutes);
}
