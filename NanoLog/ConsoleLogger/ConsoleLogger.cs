namespace NanoLog;

public sealed class ConsoleLogger : ILogger
{
    public ConsoleLogger()
    {
        
    }
    
    public void Log(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        Console.WriteLine(logEvent.Time);
    }
}