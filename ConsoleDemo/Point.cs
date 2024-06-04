using NanoLog;

namespace ConsoleDemo;

public struct Point : ILogValue
{
    public int X { get; set; }
    public int Y { get; set; }

    public void AppendMembers(ref LogMessageWriter writer)
    {
        writer.AppendInt(nameof(X), X);
        writer.AppendInt(nameof(Y), Y);
    }
}