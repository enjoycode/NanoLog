using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NanoLog;

[InterpolatedStringHandler]
public ref struct LogMessageBuilder<T> where T : ILogLevelHandler
{
    public LogMessageBuilder(int literalLength, int formattedCount, NanoLogger logger, out bool isEnabled)
    {
        IsEnabled = isEnabled = logger.IsEnabled(T.Level);
        if (isEnabled)
            return;

        _writer = new LogMessageWriter();
    }
    
    public readonly bool IsEnabled;
    private LogMessageWriter _writer;

    [UnscopedRef]
    internal ref LogMessage Message => ref _writer.Message;

    public void AppendLiteral(string s)
    {
        _writer.AppendLiteral(s);
    }

    public void AppendFormatted(DateTime value, string? format = null,
        [CallerArgumentExpression("value")] string name = "")
    {
        _writer.AppendDateTime(name, value, format);
    }
}

public interface ILogLevelHandler
{
    static abstract LogLevel Level { get; }
}

public readonly struct DebugLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Debug;
}