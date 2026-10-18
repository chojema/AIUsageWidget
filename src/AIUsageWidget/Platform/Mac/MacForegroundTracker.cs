using System.Runtime.Versioning;
using AIUsageWidget.Tracking;
using static AIUsageWidget.Platform.Mac.MacInterop;

namespace AIUsageWidget.Platform.Mac;

/// <summary>
/// Finds the frontmost app with NSWorkspace and its front window bounds with CGWindowList.
/// Only window bounds and owner process ids are read; window titles and contents are not.
/// </summary>
[SupportedOSPlatform("macos")]
public sealed class MacForegroundTracker : IForegroundTracker
{
    /// <summary>A screen in Avalonia pixel coordinates.</summary>
    public readonly record struct ScreenInfo(ScreenRect Bounds, ScreenRect WorkArea, double Scale);

    private readonly Func<IReadOnlyList<ScreenInfo>> _screens;
    private readonly HashSet<string> _extraBundleIds;
    private readonly int _ownPid = Environment.ProcessId;
    private readonly nint _workspaceClass = objc_getClass("NSWorkspace");
    private readonly nint _selShared = sel_registerName("sharedWorkspace");
    private readonly nint _selFrontmost = sel_registerName("frontmostApplication");
    private readonly nint _selBundleId = sel_registerName("bundleIdentifier");
    private readonly nint _selPid = sel_registerName("processIdentifier");
    private readonly nint _keyOwnerPid = CFStringCreateWithCString(0, "kCGWindowOwnerPID", kCFStringEncodingUTF8);
    private readonly nint _keyLayer = CFStringCreateWithCString(0, "kCGWindowLayer", kCFStringEncodingUTF8);
    private readonly nint _keyBounds = CFStringCreateWithCString(0, "kCGWindowBounds", kCFStringEncodingUTF8);

    public MacForegroundTracker(Func<IReadOnlyList<ScreenInfo>> screens, IEnumerable<string> extraBundleIds)
    {
        _screens = screens;
        _extraBundleIds = new HashSet<string>(extraBundleIds, StringComparer.OrdinalIgnoreCase);
    }

    public ForegroundInfo Poll()
    {
        nint workspace = SendPtr(_workspaceClass, _selShared);
        nint app = SendPtr(workspace, _selFrontmost);
        if (app == 0)
        {
            return ForegroundInfo.Other;
        }

        int pid = SendInt(app, _selPid);
        if (pid == _ownPid)
        {
            return ForegroundInfo.Self;
        }

        string? bundleId = NSStringToString(SendPtr(app, _selBundleId));
        if (bundleId is null ||
            !(TerminalCatalog.MacBundleIds.Contains(bundleId) || _extraBundleIds.Contains(bundleId)))
        {
            return ForegroundInfo.Other;
        }

        // Minimized windows are not on screen, so no match means the panel should hide.
        if (FrontWindowBounds(pid) is not { } points)
        {
            return ForegroundInfo.Other;
        }

        return ToPixels(points) is { } terminal
            ? new ForegroundInfo(ForegroundKind.Terminal, terminal)
            : ForegroundInfo.Other;
    }

    private CGRect? FrontWindowBounds(int pid)
    {
        nint list = CGWindowListCopyWindowInfo(kCGWindowListOptionOnScreenOnly | kCGWindowListExcludeDesktopElements, 0);
        if (list == 0)
        {
            return null;
        }
        try
        {
            long count = CFArrayGetCount(list);
            for (long i = 0; i < count; i++) // Ordered front to back.
            {
                nint window = CFArrayGetValueAtIndex(list, i);
                if (!CFNumberGetValue(CFDictionaryGetValue(window, _keyOwnerPid), kCFNumberSInt32Type, out int owner) ||
                    owner != pid ||
                    !CFNumberGetValue(CFDictionaryGetValue(window, _keyLayer), kCFNumberSInt32Type, out int layer) ||
                    layer != 0)
                {
                    continue;
                }
                if (CGRectMakeWithDictionaryRepresentation(CFDictionaryGetValue(window, _keyBounds), out CGRect rect) &&
                    rect.Width > 40 && rect.Height > 40)
                {
                    return rect;
                }
            }
            return null;
        }
        finally
        {
            CFRelease(list);
        }
    }

    /// <summary>CGWindow bounds are in points from the top-left of the main display.</summary>
    private TerminalWindow? ToPixels(CGRect points)
    {
        double centerX = points.X + points.Width / 2;
        double centerY = points.Y + points.Height / 2;
        foreach (ScreenInfo screen in _screens())
        {
            double s = screen.Scale;
            double left = screen.Bounds.X / s, top = screen.Bounds.Y / s;
            if (centerX >= left && centerX < left + screen.Bounds.Width / s &&
                centerY >= top && centerY < top + screen.Bounds.Height / s)
            {
                var bounds = new ScreenRect((int)(points.X * s), (int)(points.Y * s),
                    (int)(points.Width * s), (int)(points.Height * s));
                return new TerminalWindow(bounds, screen.WorkArea, s);
            }
        }
        return null;
    }
}
