namespace NanoLog;

public interface ILogger
{
    void Log(ref readonly LogEvent logEvent, ref readonly LogMessage message);

    void Flush();
}