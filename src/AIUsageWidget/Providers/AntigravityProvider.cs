using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

/// <summary>
/// Antigravity CLI shows its quota only in the interactive <c>/usage</c> panel. There is no
/// documented command or API for reading it automatically, so this provider reports that state
/// instead of using undocumented endpoints or stored credentials.
/// </summary>
public sealed class AntigravityProvider : IUsageProvider
{
    public ProviderId Id => ProviderId.Antigravity;

    public ProviderSnapshot Current { get; private set; } = ProviderSnapshot.Of(ProviderId.Antigravity, ProviderState.Starting);

    public event Action<ProviderSnapshot>? Changed;

    public void Start() => Refresh();

    public void Refresh()
    {
        _ = Task.Run(() =>
        {
            Current = ProviderSnapshot.Of(Id,
                CommandLocator.Exists("agy") ? ProviderState.Unsupported : ProviderState.NotInstalled);
            Changed?.Invoke(Current);
        });
    }

    public void Dispose()
    {
    }
}
