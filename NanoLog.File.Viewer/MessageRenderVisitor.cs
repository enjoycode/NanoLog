using System.Globalization;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace NanoLog.File.Viewer;

internal sealed class MessageRenderVisitor : LogMessageVisitor
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

    private void BeforeToken(ReadOnlySpan<char> name)
    {
        if (IsLogValueMember)
        {
            if (!IsFirstMember)
                Driver.AddChars(", ");

            Driver.AddChars(name);
            Driver.AddChars(": ");
        }

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
        BeforeToken(name);
        Driver.AddStr("NULL");
        AfterToken();
        return false;
    }

    protected override bool VisitBool(ReadOnlySpan<char> name, bool value)
    {
        BeforeToken(name);
        Driver.AddStr(value ? "True" : "False");
        AfterToken();
        return false;
    }

    protected override bool VisitChar(ReadOnlySpan<char> name, char value)
    {
        BeforeToken(name);
        Driver.AddRune('\'');
        Driver.AddRune(value);
        Driver.AddRune('\'');
        AfterToken();
        return false;
    }

    protected override bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value)
    {
        BeforeToken(name);
        Driver.AddStr(value.ToString());
        AfterToken();
        return false;
    }

    protected override bool VisitDouble(ReadOnlySpan<char> name, ReadOnlySpan<char> format, double value)
    {
        BeforeToken(name);
        Driver.AddStr(value.ToString(CultureInfo.InvariantCulture));
        AfterToken();
        return false;
    }

    protected override bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        BeforeToken(name);
        Driver.AddStr(value.ToString("yyyy-MM-dd HH:mm:ss"));
        AfterToken();
        return false;
    }

    protected override bool VisitGuid(ReadOnlySpan<char> name, Guid value)
    {
        BeforeToken(name);
        Driver.AddStr(value.ToString());
        AfterToken();
        return false;
    }

    protected override bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        BeforeToken(name);
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