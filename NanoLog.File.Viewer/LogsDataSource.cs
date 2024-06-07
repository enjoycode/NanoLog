using System.Collections;
using System.Runtime.InteropServices;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace NanoLog.File.Viewer;

internal sealed class LogsDataSource : IListDataSource, IList
{
    public LogsDataSource(LogList list)
    {
        _list = list;
    }

    private readonly LogList _list;
    private static readonly MessageRenderVisitor RenderVisitor = new();

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


        ref readonly var logEvent = ref _list.GetLogEvent(line);
        ref readonly var logMessage = ref _list.GetLogMessage(line);

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
        RenderVisitor.Driver = driver;
        RenderVisitor.Visit(in logMessage);
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

    public IList ToList() => this;

    #region ====IList====
    //以下都不支持，仅为了适配IListDataSource返回，可提议Terminal修改IListDataSource接口
    
    public IEnumerator GetEnumerator() => throw new NotSupportedException();

    int IList.Add(object? value) => throw new NotSupportedException();

    void IList.Clear() => throw new NotSupportedException();

    public void CopyTo(Array array, int index) => throw new NotSupportedException();

    bool IList.Contains(object? value) => throw new NotSupportedException();

    int IList.IndexOf(object? value) => throw new NotSupportedException();

    void IList.Insert(int index, object? value) => throw new NotSupportedException();

    void IList.Remove(object? value) => throw new NotSupportedException();
    void IList.RemoveAt(int index) => throw new NotSupportedException();

    bool IList.IsFixedSize => true;
    bool IList.IsReadOnly => true;

    public bool IsSynchronized => false;
    public object SyncRoot => this;

    public object? this[int index]
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    #endregion
}