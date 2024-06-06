using Terminal.Gui;

namespace NanoLog.File.Viewer;

internal static class DriverExtensions
{
    public static void AddChars(this ConsoleDriver driver, ReadOnlySpan<char> chars)
    {
        foreach (var c in chars)
        {
            driver.AddRune(c);
        }
    }
}