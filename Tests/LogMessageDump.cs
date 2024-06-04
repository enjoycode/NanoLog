using NanoLog;

namespace Tests;

public sealed class LogMessageDump : LogMessageVisitor
{
    protected override void VisitLiteral(ReadOnlySpan<char> chars)
    {
        Console.WriteLine($"Literal: {chars}");
    }

    protected override void VisitNull(ReadOnlySpan<char> name)
    {
        Console.WriteLine($"{name} is NULL");
    }

    protected override void VisitBool(ReadOnlySpan<char> name, bool value)
    {
        Console.WriteLine($"{name}={value}");
    }

    protected override void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        Console.WriteLine($"DateTime: Name=\"{name}\" Format=\"{format}\" Value={value.ToLocalTime()}");
    }
}