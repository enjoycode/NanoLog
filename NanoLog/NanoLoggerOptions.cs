using System.Diagnostics.CodeAnalysis;

namespace NanoLog;

public sealed class NanoLoggerOptions
{
    private readonly List<ILogger> _loggers = [];
    private readonly Dictionary<string, LogLevel> _filters = [];
    private int _version;

    public List<ILogger> Loggers => _loggers;

    internal int Version => _version;

    public NanoLoggerOptions AddLogger(ILogger logger)
    {
        _loggers.Add(logger);
        return this;
    }

    public NanoLoggerOptions AddFilter(string category, LogLevel level)
    {
        _filters[category] = level;
        _version++;
        return this;
    }

    internal bool TryGetFilter(string category, [MaybeNullWhen(false)] out LogLevel level) =>
        _filters.TryGetValue(category, out level);
}