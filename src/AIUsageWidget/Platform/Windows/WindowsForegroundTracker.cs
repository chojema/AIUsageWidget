using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AIUsageWidget.Tracking;

namespace AIUsageWidget.Platform.Windows;

[SupportedOSPlatform("windows")]
public sealed class WindowsForegroundTracker : IForegroundTracker
{
    private readonly HashSet<string> _extraProcesses;
    private readonly Dictionary<uint, string> _processNames = new();
    private readonly uint _ownProcessId = (uint)Environment.ProcessId;

    public WindowsForegroundTracker(IEnumerable<string> extraProcesses)
    {
        _extraProcesses = new HashSet<string>(extraProcesses, StringComparer.OrdinalIgnoreCase);
    }

    public ForegroundInfo Poll()
    {
        nint hwnd = Win32.GetForegroundWindow();
        if (hwnd == 0)
        {
            return ForegroundInfo.Other;
        }

        Win32.GetWindowThreadProcessId(hwnd, out uint pid);
        if (pid == _ownProcessId)
        {
            return ForegroundInfo.Self;
        }

        if (!IsTerminal(hwnd, pid) || Win32.IsIconic(hwnd) || !Win32.IsWindowVisible(hwnd) || IsCloaked(hwnd))
        {
            return ForegroundInfo.Other;
        }

        if (!TryGetBounds(hwnd, out ScreenRect bounds) || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return ForegroundInfo.Other;
        }

        var info = new Win32.MONITORINFO { cbSize = Marshal.SizeOf<Win32.MONITORINFO>() };
        nint monitor = Win32.MonitorFromWindow(hwnd, Win32.MONITOR_DEFAULTTONEAREST);
        if (!Win32.GetMonitorInfo(monitor, ref info))
        {
            return ForegroundInfo.Other;
        }

        uint dpi = Win32.GetDpiForWindow(hwnd);
        double scale = dpi > 0 ? dpi / 96.0 : 1.0;
        return new ForegroundInfo(ForegroundKind.Terminal, new TerminalWindow(bounds, ToRect(info.rcWork), scale));
    }

    private bool IsTerminal(nint hwnd, uint pid)
    {
        if (TerminalCatalog.WindowsClasses.Contains(Win32.ClassName(hwnd)))
        {
            return true;
        }
        string name = ProcessName(pid);
        return TerminalCatalog.WindowsProcesses.Contains(name) || _extraProcesses.Contains(name);
    }

    private string ProcessName(uint pid)
    {
        if (_processNames.TryGetValue(pid, out string? name))
        {
            return name;
        }
        try
        {
            using var process = Process.GetProcessById((int)pid);
            name = process.ProcessName;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            name = string.Empty;
        }
        if (_processNames.Count > 512)
        {
            _processNames.Clear();
        }
        _processNames[pid] = name;
        return name;
    }

    private static bool IsCloaked(nint hwnd) =>
        Win32.DwmGetWindowAttributeInt(hwnd, Win32.DWMWA_CLOAKED, out int cloaked, sizeof(int)) == 0 && cloaked != 0;

    /// <summary>Visible frame bounds without the invisible resize borders.</summary>
    private static bool TryGetBounds(nint hwnd, out ScreenRect bounds)
    {
        if (Win32.DwmGetWindowAttribute(hwnd, Win32.DWMWA_EXTENDED_FRAME_BOUNDS, out Win32.RECT frame,
                Marshal.SizeOf<Win32.RECT>()) == 0 ||
            Win32.GetWindowRect(hwnd, out frame))
        {
            bounds = ToRect(frame);
            return true;
        }
        bounds = default;
        return false;
    }

    private static ScreenRect ToRect(Win32.RECT r) => new(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
}
