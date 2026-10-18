using System.Reflection;

namespace AIUsageWidget.Core;

public static class AppInfo
{
    public const string Name = "AIUsageWidget";

    public const string RepositoryUrl = "https://github.com/chojema/AIUsageWidget";

    public static string Version { get; } =
        typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            .Split('+')[0]
        ?? "0.0.0";

    /// <summary>Full path of the running executable.</summary>
    public static string ExecutablePath => Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, Name);
}
