using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using AIUsageWidget.Ime;
using static AIUsageWidget.Platform.Mac.MacInterop;

namespace AIUsageWidget.Platform.Mac;

/// <summary>Reads the identifier of the current keyboard input source (Text Input Sources).</summary>
[SupportedOSPlatform("macos")]
public sealed class MacImeReader : IImeReader
{
    private const string Carbon = "/System/Library/Frameworks/Carbon.framework/Carbon";

    [DllImport(Carbon)]
    private static extern nint TISCopyCurrentKeyboardInputSource();

    [DllImport(Carbon)]
    private static extern nint TISGetInputSourceProperty(nint source, nint key);

    [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CFStringGetCString(nint value, byte[] buffer, long size, uint encoding);

    private readonly nint _sourceIdKey;

    public MacImeReader()
    {
        // kTISPropertyInputSourceID is an exported CFStringRef variable, not a function.
        if (NativeLibrary.TryLoad(Carbon, out nint carbon) &&
            NativeLibrary.TryGetExport(carbon, "kTISPropertyInputSourceID", out nint address))
        {
            _sourceIdKey = Marshal.ReadIntPtr(address);
        }
    }

    public string? Read()
    {
        if (_sourceIdKey == 0)
        {
            return null;
        }

        nint source = TISCopyCurrentKeyboardInputSource();
        if (source == 0)
        {
            return null;
        }
        try
        {
            nint id = TISGetInputSourceProperty(source, _sourceIdKey);
            byte[] buffer = new byte[256];
            string? text = id != 0 && CFStringGetCString(id, buffer, buffer.Length, kCFStringEncodingUTF8)
                ? Encoding.UTF8.GetString(buffer, 0, Array.IndexOf(buffer, (byte)0) is var n and >= 0 ? n : buffer.Length)
                : null;
            return ImeLabel.FromMac(text);
        }
        finally
        {
            CFRelease(source);
        }
    }
}
