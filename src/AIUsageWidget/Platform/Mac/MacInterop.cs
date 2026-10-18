using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AIUsageWidget.Platform.Mac;

[SupportedOSPlatform("macos")]
internal static class MacInterop
{
    private const string ObjC = "/usr/lib/libobjc.A.dylib";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string CoreGraphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    public const uint kCGWindowListOptionOnScreenOnly = 1 << 0;
    public const uint kCGWindowListExcludeDesktopElements = 1 << 4;
    public const int kCFNumberSInt32Type = 3;
    public const uint kCFStringEncodingUTF8 = 0x08000100;

    [StructLayout(LayoutKind.Sequential)]
    public struct CGRect
    {
        public double X, Y, Width, Height;
    }

    [DllImport(ObjC)]
    public static extern nint objc_getClass(string name);

    [DllImport(ObjC)]
    public static extern nint sel_registerName(string name);

    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    public static extern nint SendPtr(nint receiver, nint selector);

    [DllImport(ObjC, EntryPoint = "objc_msgSend")]
    public static extern int SendInt(nint receiver, nint selector);

    [DllImport(CoreFoundation)]
    public static extern nint CFStringCreateWithCString(nint allocator, string value, uint encoding);

    [DllImport(CoreFoundation)]
    public static extern long CFArrayGetCount(nint array);

    [DllImport(CoreFoundation)]
    public static extern nint CFArrayGetValueAtIndex(nint array, long index);

    [DllImport(CoreFoundation)]
    public static extern nint CFDictionaryGetValue(nint dictionary, nint key);

    [DllImport(CoreFoundation)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool CFNumberGetValue(nint number, int type, out int value);

    [DllImport(CoreFoundation)]
    public static extern void CFRelease(nint value);

    [DllImport(CoreGraphics)]
    public static extern nint CGWindowListCopyWindowInfo(uint option, uint relativeToWindow);

    [DllImport(CoreGraphics)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool CGRectMakeWithDictionaryRepresentation(nint dictionary, out CGRect rect);

    public static string? NSStringToString(nint nsString)
    {
        if (nsString == 0)
        {
            return null;
        }
        nint utf8 = SendPtr(nsString, sel_registerName("UTF8String"));
        return utf8 == 0 ? null : Marshal.PtrToStringUTF8(utf8);
    }
}
