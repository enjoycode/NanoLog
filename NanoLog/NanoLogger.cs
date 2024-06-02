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

    private readonly string? _categoryName;

    public NanoLogger(string? categoryName = null)
    {
        _categoryName = categoryName;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        //TODO:
        return true;
    }

    public void Debug([InterpolatedStringHandlerArgument("")] ref LogMessageBuilder<DebugLevel> builder,
        [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
    {
        if (!builder.IsEnabled) return;

        builder.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Debug, file, member, line);
        _processor.Enqueue(ref logEvent, ref builder.Message);
    }
}