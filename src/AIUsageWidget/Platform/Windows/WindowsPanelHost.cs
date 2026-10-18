using System.Runtime.Versioning;
using AIUsageWidget.Tracking;

namespace AIUsageWidget.Platform.Windows;

/// <summary>Native window settings for the panel on Windows.</summary>
[SupportedOSPlatform("windows")]
public static class WindowsPanelHost
{
    /// <summary>Keeps the panel out of Alt+Tab and the taskbar, and stops clicks from taking focus.</summary>
    public static void MakeNonActivating(nint hwnd)
    {
        long style = Win32.GetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE);
        style |= Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW;
        style &= ~Win32.WS_EX_APPWINDOW;
        Win32.SetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE, (nint)style);
    }

    /// <summary>Moves and sizes the panel in physical pixels without activating it.</summary>
    public static void Place(nint hwnd, ScreenRect rect) =>
        Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, rect.X, rect.Y, rect.Width, rect.Height,
            Win32.SWP_NOACTIVATE | Win32.SWP_NOOWNERZORDER);
}
