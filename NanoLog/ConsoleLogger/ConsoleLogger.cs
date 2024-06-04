using System.Runtime.InteropServices;

namespace NanoLog;

public sealed class ConsoleLogger : ILogger
{
    public ConsoleLogger(ConsoleFormatter? formatter = null)
    {
        //var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var output = Console.OpenStandardOutput();
        _formatter = formatter ?? new DefaultConsoleFormatter(output);
    }

    private readonly ConsoleFormatter _formatter;

    public void Log(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        _formatter.Output(in logEvent, in message);
    }
}