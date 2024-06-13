using NanoLog;

namespace ConsoleDemo;

public readonly struct NodeId(ulong value) : ILogValue
{
    public bool IsScalar => true;
    
    public void AppendMembers(ref LogMessageWriter writer, string name)
    {
        writer.AppendULong(name, value, "X");
    }
}