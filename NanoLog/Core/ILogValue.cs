namespace NanoLog;

public interface ILogValue
{
    bool IsScalar { get; }

    void AppendMembers(ref LogMessageWriter writer, string name);
}