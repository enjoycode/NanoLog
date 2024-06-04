using BenchmarkDotNet.Attributes;
using NanoLog;

namespace Benchmark;

[MemoryDiagnoser]
public class FormatterBenchmark
{
    private ConsoleLogger _logger = null!;
    private LogMessage _message;
    private LogEvent _logEvent;

    [GlobalSetup]
    public void Setup()
    {
        _logger = new ConsoleLogger();
        var msgBuilder = new LogMessageBuilder<DebugLevel>();
        msgBuilder.AppendLiteral("Hello World ");
        msgBuilder.AppendFormatted(DateTime.Now, null, "Now");
        msgBuilder.FinishWrite();
        _message = msgBuilder.Message;
        _logEvent = new LogEvent(LogLevel.Debug, "", "Program.cs", "Main", 10);
    }

    [Benchmark]
    public void FormatBenchmark()
    {
        _logger.Log(ref _logEvent, ref _message);
    }
}