namespace AIUsageWidget.Core;

public enum ProviderId
{
    Codex,
    ClaudeCode,
    Antigravity,
}

public enum ProviderState
{
    /// <summary>Nothing has been read yet.</summary>
    Starting,
    /// <summary>At least one usage window is known.</summary>
    Ok,
    /// <summary>The CLI could not be found on this computer.</summary>
    NotInstalled,
    /// <summary>The CLI is installed but reports no signed-in account.</summary>
    NotSignedIn,
    /// <summary>The integration needs to be enabled by the user first.</summary>
    NotConnected,
    /// <summary>Connected, but the CLI has not reported limits yet.</summary>
    WaitingForData,
    /// <summary>No official interface exists for automatic collection.</summary>
    Unsupported,
    Error,
}

/// <summary>One rate-limit window, such as the 5-hour or the weekly limit.</summary>
public sealed record UsageWindow(double UsedPercent, DateTimeOffset? ResetsAt)
{
    public double RemainingPercent => Math.Clamp(100.0 - UsedPercent, 0.0, 100.0);

    public bool HasExpired(DateTimeOffset now) => ResetsAt is { } r && r <= now;
}

public sealed record ProviderSnapshot(
    ProviderId Provider,
    ProviderState State,
    UsageWindow? FiveHour = null,
    UsageWindow? Weekly = null,
    DateTimeOffset? UpdatedAt = null,
    string? Detail = null,
    string? Label = null)
{
    public static ProviderSnapshot Of(ProviderId id, ProviderState state, string? detail = null) =>
        new(id, state, Detail: detail);
}
