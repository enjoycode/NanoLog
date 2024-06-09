using System.Buffers;
using System.Runtime.InteropServices;

namespace NanoLog;

public sealed class LogList : IDisposable
{
    public LogList(List<(LogEvent, LogMessage)> source, Action<byte[]>? freeOuterData = null)
    {
        Source = source;
        _freeOuterData = freeOuterData ?? (d => ArrayPool<byte>.Shared.Return(d));
    }

    public readonly List<(LogEvent, LogMessage)> Source;
    private readonly Action<byte[]> _freeOuterData;


    public int Count => Source.Count;
    public ref readonly LogEvent GetLogEvent(int row) => ref CollectionsMarshal.AsSpan(Source)[row].Item1;
    public ref readonly LogMessage GetLogMessage(int row) => ref CollectionsMarshal.AsSpan(Source)[row].Item2;

    public void Dispose()
    {
        for (var i = 0; i < Source.Count; i++)
        {
            ref readonly var msg = ref GetLogMessage(i);
            if (msg.OuterData != null)
                _freeOuterData(msg.OuterData);
        }

        Source.Clear();
    }
}