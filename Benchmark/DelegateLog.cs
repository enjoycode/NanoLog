using Microsoft.Extensions.Logging;

namespace Benchmark;

public static partial class DelegateLog
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Hello World {now}")]
    public static partial void Log(ILogger logger, DateTime now);
}