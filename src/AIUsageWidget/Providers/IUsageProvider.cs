using AIUsageWidget.Core;

namespace AIUsageWidget.Providers;

public interface IUsageProvider : IDisposable
{
    ProviderId Id { get; }

    ProviderSnapshot Current { get; }

    /// <summary>Raised on a background thread whenever <see cref="Current"/> changes.</summary>
    event Action<ProviderSnapshot>? Changed;

    void Start();

    /// <summary>Asks for fresh data as soon as possible.</summary>
    void Refresh();
}
