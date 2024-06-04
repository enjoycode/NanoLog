using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NanoLog;

[InterpolatedStringHandler]
public ref struct LogMessageBuilder<T> where T : ILogLevelHandler
{
    public LogMessageBuilder([SuppressMessage("ReSharper", "UnusedParameter.Local")] int literalLength,
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        int formattedCount, NanoLogger logger,
        out bool isEnabled)
    {
        IsEnabled = isEnabled = logger.IsEnabled(T.Level);
        if (isEnabled)
            return;

        _writer = new LogMessageWriter();
    }

    public readonly bool IsEnabled;
    private LogMessageWriter _writer;

    [UnscopedRef] internal ref LogMessage Message => ref _writer.Message;

    public void AppendLiteral(string s)
    {
        _writer.AppendLiteral(s);
    }

    public void AppendFormatted(bool? value, [CallerArgumentExpression("value")] string name = "")
    {
        _writer.AppendBool(name, value);
    }

    public void AppendFormatted(char? value, [CallerArgumentExpression("value")] string name = "")
    {
        _writer.AppendChar(name, value);
    }

    public void AppendFormatted(DateTime? value, string? format = null,
        [CallerArgumentExpression("value")] string name = "")
    {
        _writer.AppendDateTime(name, value, format);
    }

    public void AppendFormatted(string? value, [CallerArgumentExpression("value")] string name = "")
    {
        _writer.AppendString(name, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void FinishWrite() => _writer.FinishWrite();
}

public interface ILogLevelHandler
{
    static abstract LogLevel Level { get; }
}

public readonly struct TraceLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Trace;
}

public readonly struct DebugLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Debug;
}

public readonly struct InfoLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Information;
}

public readonly struct WarnLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Warning;
}

public readonly struct ErrorLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Error;
}

public readonly struct FatalLevel : ILogLevelHandler
{
    public static LogLevel Level => LogLevel.Fatal;
}