namespace AIUsageWidget.Ime;

public interface IImeReader
{
    /// <summary>The input state of the window in front, or null when it cannot be read.</summary>
    string? Read();
}
