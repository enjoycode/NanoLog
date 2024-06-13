using NanoLog;

namespace ConsoleDemo;

public struct Point : ILogValue
{
    public int X { get; set; }
    public int Y { get; set; }

    bool ILogValue.IsScalar => false;

    public void AppendMembers(ref LogMessageWriter writer, string name)
    {
        writer.AppendInt(nameof(X), X);
        writer.AppendInt(nameof(Y), Y);
    }
}