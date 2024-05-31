using NanoLog;

namespace Tests;

public sealed class LogMessageDump : LogMessageVisitor
{
    protected override void VisitLiteral(ReadOnlySpan<char> chars)
    {
        Console.WriteLine($"Literal: {chars}");
    }

    protected override void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        Console.WriteLine($"DateTime: Name=\"{name}\" Format=\"{format}\" Value={value.ToLocalTime()}");
    }
}