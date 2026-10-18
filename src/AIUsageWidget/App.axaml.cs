using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AIUsageWidget;

public sealed class App : Application
{
    private AppController? _controller;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _controller = new AppController(this, desktop);
            desktop.Exit += (_, _) => _controller.Dispose();
            _controller.Start();
            if (desktop.Args?.Contains("--settings") == true)
            {
                _controller.OpenSettings();
            }
        }
        base.OnFrameworkInitializationCompleted();
    }
}
