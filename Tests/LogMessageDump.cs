using NanoLog;

namespace Tests;

public sealed class LogMessageDump : LogMessageVisitor
{
    protected override bool VisitLiteral(ReadOnlySpan<char> chars)
    {
        Console.WriteLine($"Literal: {chars}");
        return false;
    }

    protected override bool VisitNull(ReadOnlySpan<char> name)
    {
        Console.WriteLine($"{name} is NULL");
        return false;
    }

    protected override bool VisitBool(ReadOnlySpan<char> name, bool value)
    {
        Console.WriteLine($"{name}={value}");
        return false;
    }

    protected override bool VisitChar(ReadOnlySpan<char> name, char value)
    {
        Console.WriteLine($"{name}={value}");
        return false;
    }

    protected override bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value)
    {
        Console.WriteLine($"Int: Name=\"{name}\" Format=\"{format}\" Value={value}");
        return false;
    }

    protected override bool VisitDouble(ReadOnlySpan<char> name, ReadOnlySpan<char> format, double value)
    {
        Console.WriteLine($"Double: Name=\"{name}\" Format=\"{format}\" Value={value}");
        return false;
    }

    protected override bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        Console.WriteLine($"DateTime: Name=\"{name}\" Format=\"{format}\" Value={value.ToLocalTime()}");
        return false;
    }

    protected override bool VisitGuid(ReadOnlySpan<char> name, Guid value)
    {
        Console.WriteLine($"{name}={value}");
        return false;
    }

    protected override bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        Console.WriteLine($"{name}=\"{value}\"");
        return false;
    }

    protected override bool BeginVisitLogValue(ReadOnlySpan<char> name)
    {
        Console.Write("{");
        return false;
    }

    protected override bool EndVisitLogValue()
    {
        Console.Write("}");
        return false;
    }
}