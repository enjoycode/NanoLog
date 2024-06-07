namespace NanoLog;

public interface ILogValue
{
    void AppendMembers(ref LogMessageWriter writer);
}