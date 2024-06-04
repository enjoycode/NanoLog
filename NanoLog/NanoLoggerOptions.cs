namespace NanoLog;

public sealed class NanoLoggerOptions
{
    private readonly List<ILogger> _loggers = [];

    public List<ILogger> Loggers => _loggers;
    
    public void AddLogger(ILogger logger) => _loggers.Add(logger);
}