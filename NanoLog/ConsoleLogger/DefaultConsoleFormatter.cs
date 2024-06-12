namespace NanoLog;

public sealed class DefaultConsoleFormatter : ConsoleFormatter
{
    public DefaultConsoleFormatter(Stream output, string timestampFormat = "yyyy-MM-dd hh:mm:ss.fff")
        : base(output, timestampFormat)
    {
        _visitor = new ConsoleMessageVisitor(this);
    }

    //1B5B33316D [=5B 3=33 1=31 m=6D
    private static readonly byte[] Red = [0x1B, 0x5B, 0x33, 0x31, 0x6D];
    private static readonly byte[] Green = [0x1B, 0x5B, 0x33, 0x32, 0x6D];
    private static readonly byte[] Yellow = [0x1B, 0x5B, 0x33, 0x33, 0x6D];
    private static readonly byte[] Blue = [0x1B, 0x5B, 0x33, 0x34, 0x6D];
    private static readonly byte[] Cyan = [0x1B, 0x5B, 0x33, 0x36, 0x6D];
    private static readonly byte[] Magenta = [0x1B, 0x5B, 0x33, 0x35, 0x6D];
    private static readonly byte[] Reset = [0x1B, 0x5B, 0x30, 0x6D];

    internal static char GetLevelChar(LogLevel level) => level switch
    {
        LogLevel.Trace => 'T',
        LogLevel.Debug => 'D',
        LogLevel.Information => 'I',
        LogLevel.Warning => 'W',
        LogLevel.Error => 'E',
        LogLevel.Fatal => 'F',
        _ => 'U',
    };

    private static byte[] GetLevelColor(LogLevel level) => level switch
    {
        LogLevel.Trace => Cyan,
        LogLevel.Debug => Blue,
        LogLevel.Information => Green,
        LogLevel.Warning => Yellow,
        LogLevel.Error => Red,
        LogLevel.Fatal => Red,
        _ => Magenta,
    };

    private readonly ConsoleMessageVisitor _visitor;

    public override void Output(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        //Text color by level
        var color = GetLevelColor(logEvent.Level);
        WriteBytes(color);

        //Timestamp
        WriteByte((byte)'[');
        WriteByte((byte)GetLevelChar(logEvent.Level));
        WriteFormattable(logEvent.Time.ToLocalTime(), TimestampFormat);

        //File source
        WriteByte((byte)' ');
        WriteChars(Path.GetFileName(logEvent.File.AsSpan()));
        WriteByte((byte)'.');
        WriteChars(logEvent.Member);
        WriteByte((byte)':');
        WriteFormattable(logEvent.Line);

        WriteByte((byte)']');
        WriteByte((byte)' ');

        //Reset text color
        WriteBytes(Reset);

        //Message
        _visitor.Visit(in message);

        //New line
        WriteByte((byte)'\n');

        FlushBuffer();
    }

    protected internal override void BeforeToken(TokenType tokenType)
    {
        WriteBytes(Magenta);
    }

    protected internal override void AfterToken(TokenType tokenType)
    {
        WriteBytes(Reset);
    }
}