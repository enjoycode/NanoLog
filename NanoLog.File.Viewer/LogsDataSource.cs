using System.Collections;
using System.Runtime.InteropServices;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace NanoLog.File.Viewer;

internal sealed class LogsDataSource : IListDataSource
{
    public LogsDataSource(List<(LogEvent, LogMessage)> list)
    {
        _list = list;
    }

    private readonly List<(LogEvent, LogMessage)> _list;
    private static readonly TuiMessageVisitor _visitor = new();

    public int Count => _list.Count;
    public int Length => 800; //TODO:

    public bool IsMarked(int item)
    {
        return false;
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected,
        int item, int col, int line, int width, int start = 0)
    {
        container.Move(Math.Max(col - start, 0), line);

        var span = CollectionsMarshal.AsSpan(_list);
        ref var logEvent = ref span[line].Item1;
        ref var logMessage = ref span[line].Item2;

        //TODO: maybe use bytes buffer?
        SetColorByLevel(driver, out var current, logEvent.Level);
        driver.AddRune('[');
        WriteLevel(driver, logEvent.Level);
        //Timestamp
        driver.AddStr(logEvent.Time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff"));

        //File
        driver.AddRune(' ');
        driver.AddChars(Path.GetFileName(logEvent.File.AsSpan()));
        // driver.AddRune('.');
        // driver.AddStr(logEvent.Member);
        driver.AddRune(':');
        driver.AddStr(logEvent.Line.ToString());

        driver.AddRune(']');
        driver.AddRune(' ');

        //Reset color
        driver.SetAttribute(current);

        //Message
        _visitor.Driver = driver;
        _visitor.Visit(ref logMessage);
    }

    private static void WriteLevel(ConsoleDriver driver, LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                driver.AddRune('T');
                break;
            case LogLevel.Debug:
                driver.AddRune('D');
                break;
            case LogLevel.Information:
                driver.AddRune('I');
                break;
            case LogLevel.Warning:
                driver.AddRune('W');
                break;
            case LogLevel.Error:
                driver.AddRune('E');
                break;
            case LogLevel.Fatal:
                driver.AddRune('F');
                break;
        }
    }

    private static void SetColorByLevel(ConsoleDriver driver, out Attribute current, LogLevel level)
    {
        //TODO: by theme
        current = driver.CurrentAttribute;
        switch (level)
        {
            case LogLevel.Trace:
                driver.SetAttribute(driver.MakeColor(Color.BrightCyan, current.Background));
                break;
            case LogLevel.Debug:
                driver.SetAttribute(driver.MakeColor(Color.BrightBlue, current.Background));
                break;
            case LogLevel.Information:
                driver.SetAttribute(driver.MakeColor(Color.BrightGreen, current.Background));
                break;
            case LogLevel.Warning:
                driver.SetAttribute(driver.MakeColor(Color.BrightYellow, current.Background));
                break;
            case LogLevel.Error:
                driver.SetAttribute(driver.MakeColor(Color.BrightRed, current.Background));
                break;
            case LogLevel.Fatal:
                driver.SetAttribute(driver.MakeColor(Color.BrightRed, current.Background));
                break;
        }
    }

    public void SetMark(int item, bool value)
    {
    }

    public IList ToList() => _list;
}