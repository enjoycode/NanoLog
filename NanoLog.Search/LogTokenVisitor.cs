namespace NanoLog.File.Viewer;

/// <summary>
/// 用于抽取LogMessage内的[属性-值]字典表
/// </summary>
public sealed class LogTokenVisitor : LogMessageVisitor
{
    private LogTokenNode _root = null!;
    private LogTokenNode _current = null!;

    public void BeforeVisit()
    {
        _root = new LogTokenNode(null);
        _current = _root;
    }

    public LogTokenNode AfterVisit()
    {
        var res = _root;
        _root = _current = null!;
        return res;
    }

    protected override bool VisitLiteral(ReadOnlySpan<char> chars) => false;

    protected override bool VisitNull(ReadOnlySpan<char> name)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.Null, null));
        return false;
    }

    protected override bool VisitBool(ReadOnlySpan<char> name, bool value)
    {
        _current.TryAddChild(name.ToString(),
            new LogTokenNode(_current, value ? TokenType.BoolTrue : TokenType.BoolFalse, value));
        return false;
    }

    protected override bool VisitChar(ReadOnlySpan<char> name, char value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.Char, value));
        return false;
    }

    protected override bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.Int, value));
        return false;
    }

    protected override bool VisitULong(ReadOnlySpan<char> name, ReadOnlySpan<char> format, ulong value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.ULong, value));
        return false;
    }

    protected override bool VisitDouble(ReadOnlySpan<char> name, ReadOnlySpan<char> format, double value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.Double, value));
        return false;
    }

    protected override bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.DateTime, value));
        return false;
    }

    protected override bool VisitGuid(ReadOnlySpan<char> name, Guid value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.Guid, value));
        return false;
    }

    protected override bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        _current.TryAddChild(name.ToString(), new LogTokenNode(_current, TokenType.String4, value.ToString()));
        return false;
    }

    protected override bool BeginVisitLogValue(ReadOnlySpan<char> name)
    {
        var child = new LogTokenNode(_current);
        _current.TryAddChild(name.ToString(), child);
        _current = child;
        return false;
    }

    protected override bool EndVisitLogValue()
    {
        _current = _current.Parent!;
        return false;
    }
}