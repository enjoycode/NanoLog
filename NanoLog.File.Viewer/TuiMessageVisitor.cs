using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace NanoLog.File.Viewer;

internal sealed class TuiMessageVisitor : LogMessageVisitor
{
    private ConsoleDriver _driver = null!;
    private Attribute _current;

    public ConsoleDriver Driver
    {
        get => _driver;
        set
        {
            _driver = value;
            _current = _driver.CurrentAttribute;
        }
    }

    private void BeforeToken()
    {
        Driver.SetAttribute(Driver.MakeColor(Color.Magenta, _current.Background));
    }

    private void AfterToken()
    {
        Driver.SetAttribute(_current);
    }

    protected override bool VisitLiteral(ReadOnlySpan<char> chars)
    {
        Driver.AddChars(chars);
        return false;
    }

    protected override bool VisitNull(ReadOnlySpan<char> name)
    {
        BeforeToken();
        Driver.AddStr("NULL");
        AfterToken();
        return false;
    }

    protected override bool VisitBool(ReadOnlySpan<char> name, bool value)
    {
        BeforeToken();
        Driver.AddStr(value ? "True" : "False");
        AfterToken();
        return false;
    }

    protected override bool VisitChar(ReadOnlySpan<char> name, char value)
    {
        BeforeToken();
        Driver.AddRune('\'');
        Driver.AddRune(value);
        Driver.AddRune('\'');
        AfterToken();
        return false;
    }

    protected override bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value)
    {
        BeforeToken();
        Driver.AddStr(value.ToString());
        AfterToken();
        return false;
    }

    protected override bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        BeforeToken();
        Driver.AddStr(value.ToString("yyyy-MM-dd HH:mm:ss"));
        AfterToken();
        return false;
    }

    protected override bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        BeforeToken();
        Driver.AddRune('"');
        Driver.AddChars(value);
        Driver.AddRune('"');
        AfterToken();
        return false;
    }

    protected override bool BeginVisitLogValue(ReadOnlySpan<char> name)
    {
        Driver.AddRune('{');
        return false;
    }

    protected override bool EndVisitLogValue()
    {
        Driver.AddRune('}');
        return false;
    }
}