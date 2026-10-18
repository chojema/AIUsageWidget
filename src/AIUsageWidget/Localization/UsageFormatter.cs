using System.Globalization;
using AIUsageWidget.Core;

namespace AIUsageWidget.Localization;

public static class UsageFormatter
{
    public static string Remaining(UsageWindow window, Strings s) =>
        string.Format(CultureInfo.InvariantCulture, s.Left, Math.Round(window.RemainingPercent));

    /// <summary>"resets 15:40 (in 1h 12m)", or with a weekday when the reset is more than a day away.</summary>
    public static string Reset(UsageWindow window, DateTimeOffset now, Strings s)
    {
        if (window.ResetsAt is not { } resetsAt)
        {
            return string.Empty;
        }

        DateTimeOffset local = resetsAt.ToLocalTime();
        string clock = local.ToString("HH:mm", CultureInfo.InvariantCulture);
        string when = resetsAt - now > TimeSpan.FromHours(24)
            ? $"{s.Days[(int)local.DayOfWeek]} {clock}"
            : clock;
        string left = string.Format(CultureInfo.InvariantCulture, s.ResetsIn, Duration(resetsAt - now, s));
        return $"{string.Format(CultureInfo.InvariantCulture, s.Resets, when)} ({left})";
    }

    public static string Duration(TimeSpan span, Strings s)
    {
        if (span < TimeSpan.Zero)
        {
            span = TimeSpan.Zero;
        }
        int totalMinutes = (int)Math.Ceiling(span.TotalMinutes);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;
        if (hours >= 48)
        {
            int days = hours / 24;
            return s.Code == "ko" ? $"{days}일 {hours % 24}시간" : $"{days}d {hours % 24}h";
        }
        return hours > 0
            ? string.Format(CultureInfo.InvariantCulture, s.Hours, hours, minutes)
            : string.Format(CultureInfo.InvariantCulture, s.Minutes, minutes);
    }

    public static string Updated(DateTimeOffset updatedAt, DateTimeOffset now, Strings s) =>
        now - updatedAt < TimeSpan.FromMinutes(1)
            ? s.JustNow
            : string.Format(CultureInfo.InvariantCulture, s.UpdatedAgo, Duration(now - updatedAt, s));
}
