namespace NanoLog;

/// <summary>
/// Console logger for unit test
/// </summary>
public sealed class UnitTestConsoleLogger : ILogger
{
    private readonly UnitTestMessageVisitor _visitor = new();
    private readonly char[] _dateTimeBuffer = new char[32];

    public void Log(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        logEvent.Time.TryFormat(_dateTimeBuffer.AsSpan(), out var charsWritten, "MMdd hh:mm:ss.fff");

        Console.Out.Write('[');
        Console.Out.Write(DefaultConsoleFormatter.GetLevelChar(logEvent.Level));
        Console.Out.Write(_dateTimeBuffer.AsSpan(0, charsWritten));
        Console.Out.Write(' ');
        Console.Out.Write(Path.GetFileName(logEvent.File));
        Console.Out.Write(' ');
        Console.Out.Write(logEvent.Member);
        Console.Out.Write(':');
        Console.Out.Write(logEvent.Line);
        Console.Out.Write("]: ");

        _visitor.Visit(in message);
        Console.WriteLine();
    }

    public void Flush() { }
}

internal sealed class UnitTestMessageVisitor : LogMessageVisitor
{
    private void TryWriteMemberName(ReadOnlySpan<char> name)
    {
        if (IsLogValueMember)
        {
            if (!IsFirstMember)
                Console.Out.Write(", ");

            Console.Out.Write(name);
            Console.Out.Write(": ");
        }
    }

    protected override bool VisitLiteral(ReadOnlySpan<char> chars)
    {
        Console.Out.Write(chars);
        return false;
    }

    protected override bool VisitNull(ReadOnlySpan<char> name)
    {
        TryWriteMemberName(name);

        Console.Out.Write("NULL");
        return false;
    }

    protected override bool VisitBool(ReadOnlySpan<char> name, bool value)
    {
        TryWriteMemberName(name);

        Console.Out.Write(value);
        return false;
    }

    protected override bool VisitChar(ReadOnlySpan<char> name, char value)
    {
        TryWriteMemberName(name);

        Console.Out.Write('\'');
        Console.Out.Write(value);
        Console.Out.Write('\'');
        return false;
    }

    protected override bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value)
    {
        TryWriteMemberName(name);

        Console.Out.Write(value);
        return false;
    }

    protected override bool VisitULong(ReadOnlySpan<char> name, ReadOnlySpan<char> format, ulong value)
    {
        TryWriteMemberName(name);

        Console.Out.Write(value);
        return false;
    }

    protected override bool VisitDouble(ReadOnlySpan<char> name, ReadOnlySpan<char> format, double value)
    {
        TryWriteMemberName(name);

        Console.Out.Write(value);
        return false;
    }

    protected override bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        TryWriteMemberName(name);

        Console.Out.Write(value);
        return false;
    }

    protected override bool VisitGuid(ReadOnlySpan<char> name, Guid value)
    {
        TryWriteMemberName(name);

        Console.Out.Write(value);
        return false;
    }

    protected override bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        TryWriteMemberName(name);

        Console.Out.Write('"');
        Console.Out.Write(value);
        Console.Out.Write('"');
        return false;
    }

    protected override bool BeginVisitLogValue(ReadOnlySpan<char> name)
    {
        Console.Out.Write('{');
        return false;
    }

    protected override bool EndVisitLogValue()
    {
        Console.Out.Write('}');
        return false;
    }
}