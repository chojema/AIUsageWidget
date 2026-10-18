using System.Runtime.Versioning;
using Microsoft.Win32;
using AIUsageWidget.Core;

namespace AIUsageWidget.Platform.Windows;

/// <summary>Start at login through the per-user Run key. Nothing is written until the user turns it on.</summary>
[SupportedOSPlatform("windows")]
public static class WindowsStartup
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(AppInfo.Name) is string value &&
               value.Contains(AppInfo.ExecutablePath, StringComparison.OrdinalIgnoreCase);
    }

    public static void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
        {
            key.SetValue(AppInfo.Name, $"\"{AppInfo.ExecutablePath}\"");
        }
        else
        {
            key.DeleteValue(AppInfo.Name, throwOnMissingValue: false);
        }
    }
}
