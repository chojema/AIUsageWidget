using System.Globalization;

namespace AIUsageWidget.Ime;

/// <summary>Turns platform keyboard state into a short label such as "EN" or "한".</summary>
public static class ImeLabel
{
    public const string Hangul = "한";
    public const string English = "EN";

    private const int LangKorean = 0x12;
    private const int ImeCmodeNative = 0x0001; // IME_CMODE_HANGUL for the Korean IME

    /// <summary>
    /// Windows: <paramref name="languageId"/> is the low word of the keyboard layout (HKL);
    /// <paramref name="conversionMode"/> is the IME conversion mode, when it could be read.
    /// </summary>
    public static string FromWindows(int languageId, int? conversionMode)
    {
        if ((languageId & 0x3FF) == LangKorean)
        {
            return conversionMode is { } mode && (mode & ImeCmodeNative) != 0 ? Hangul : English;
        }

        try
        {
            return CultureInfo.GetCultureInfo(languageId).TwoLetterISOLanguageName.ToUpperInvariant();
        }
        catch (CultureNotFoundException)
        {
            return English;
        }
    }

    /// <summary>macOS: <paramref name="inputSourceId"/> is the current TIS input source identifier.</summary>
    public static string FromMac(string? inputSourceId)
    {
        if (string.IsNullOrEmpty(inputSourceId))
        {
            return English;
        }
        if (inputSourceId.Contains("Korean", StringComparison.OrdinalIgnoreCase))
        {
            // The Korean input method also has a Roman (English) mode.
            return inputSourceId.Contains("Roman", StringComparison.OrdinalIgnoreCase) ? English : Hangul;
        }
        if (inputSourceId.Contains("Japanese", StringComparison.OrdinalIgnoreCase) ||
            (inputSourceId.Contains("Kotoeri", StringComparison.OrdinalIgnoreCase) &&
             !inputSourceId.Contains("Roman", StringComparison.OrdinalIgnoreCase)))
        {
            return "JA";
        }
        if (inputSourceId.Contains("SCIM", StringComparison.OrdinalIgnoreCase) ||
            inputSourceId.Contains("TCIM", StringComparison.OrdinalIgnoreCase) ||
            inputSourceId.Contains("Chinese", StringComparison.OrdinalIgnoreCase))
        {
            return "ZH";
        }
        return English;
    }
}
