using System.Runtime.CompilerServices;

namespace NanoLog;

public sealed class NanoLogger
{
    #region ====Static Methods====

    private static LogProcessor _processor = null!;
    internal static NanoLoggerOptions Options { get; private set; } = null!;

    public static void Start(NanoLoggerOptions? options = null)
    {
        if (options == null)
        {
            options = new NanoLoggerOptions();
            options.AddLogger(new ConsoleLogger());
        }

        Options = options;
        _processor = new LogProcessor();
    }

    public static void Stop()
    {
        _processor.Stop();
    }

    #endregion

    private readonly string _categoryName;

    public NanoLogger(string? categoryName = null)
    {
        _categoryName = categoryName ?? "";
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        //TODO:
        return true;
    }

    #region ====Log Methods====

    public void Trace(string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0)
    {
        if (!IsEnabled(LogLevel.Trace)) return;

        var builder = new LogMessageBuilder<TraceLevel>();
        builder.AppendLiteral(message);
        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Trace, _categoryName, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }

    public void Trace([InterpolatedStringHandlerArgument("")] ref LogMessageBuilder<TraceLevel> builder,
        [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
    {
        if (!builder.IsEnabled) return;

        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Trace, _categoryName, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }

    public void Debug([InterpolatedStringHandlerArgument("")] ref LogMessageBuilder<DebugLevel> builder,
        [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
    {
        if (!builder.IsEnabled) return;

        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Debug, _categoryName, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }

    public void Info([InterpolatedStringHandlerArgument("")] ref LogMessageBuilder<InfoLevel> builder,
        [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
    {
        if (!builder.IsEnabled) return;

        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Information, _categoryName, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }

    public void Warn([InterpolatedStringHandlerArgument("")] ref LogMessageBuilder<WarnLevel> builder,
        [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
    {
        if (!builder.IsEnabled) return;

        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Warning, _categoryName, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }

    public void Error([InterpolatedStringHandlerArgument("")] ref LogMessageBuilder<ErrorLevel> builder,
        [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
    {
        if (!builder.IsEnabled) return;

        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Error, _categoryName, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }

    #endregion
}