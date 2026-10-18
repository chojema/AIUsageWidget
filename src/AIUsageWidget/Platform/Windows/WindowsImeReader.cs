using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AIUsageWidget.Ime;

namespace AIUsageWidget.Platform.Windows;

/// <summary>
/// Reads the keyboard layout and IME conversion mode of the focused window in front.
/// The conversion mode is asked from that window's default IME window, which works across processes.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed partial class WindowsImeReader : IImeReader
{
    private const uint WM_IME_CONTROL = 0x0283;
    private const nint IMC_GETCONVERSIONMODE = 0x0001;
    private const uint SMTO_ABORTIFHUNG = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct GUITHREADINFO
    {
        public int cbSize;
        public int flags;
        public nint hwndActive;
        public nint hwndFocus;
        public nint hwndCapture;
        public nint hwndMenuOwner;
        public nint hwndMoveSize;
        public nint hwndCaret;
        public Win32.RECT rcCaret;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetGUIThreadInfo(uint threadId, ref GUITHREADINFO info);

    [LibraryImport("user32.dll")]
    private static partial nint GetKeyboardLayout(uint threadId);

    [LibraryImport("imm32.dll")]
    private static partial nint ImmGetDefaultIMEWnd(nint hwnd);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageTimeoutW")]
    private static partial nint SendMessageTimeout(nint hwnd, uint msg, nint wParam, nint lParam,
        uint flags, uint timeout, out nint result);

    public string? Read()
    {
        nint foreground = Win32.GetForegroundWindow();
        if (foreground == 0)
        {
            return null;
        }

        uint thread = Win32.GetWindowThreadProcessId(foreground, out _);
        var info = new GUITHREADINFO { cbSize = Marshal.SizeOf<GUITHREADINFO>() };
        nint target = GetGUIThreadInfo(thread, ref info) && info.hwndFocus != 0 ? info.hwndFocus : foreground;

        int languageId = (int)(GetKeyboardLayout(thread) & 0xFFFF);

        int? conversion = null;
        nint imeWindow = ImmGetDefaultIMEWnd(target);
        if (imeWindow != 0 &&
            SendMessageTimeout(imeWindow, WM_IME_CONTROL, IMC_GETCONVERSIONMODE, 0, SMTO_ABORTIFHUNG, 100,
                out nint mode) != 0)
        {
            conversion = (int)mode;
        }

        return ImeLabel.FromWindows(languageId, conversion);
    }
}
