using System.Buffers;
using System.Runtime.InteropServices;

namespace NanoLog;

public sealed class LogList : IDisposable
{
    public LogList(List<(LogEvent, LogMessage)> source, Action<byte[]>? freeOuterData = null)
    {
        _source = source;
        _freeOuterData = freeOuterData ?? (d => ArrayPool<byte>.Shared.Return(d));
    }

    private readonly List<(LogEvent, LogMessage)> _source;
    private readonly Action<byte[]> _freeOuterData;


    public int Count => _source.Count;
    public ref readonly LogEvent GetLogEvent(int row) => ref CollectionsMarshal.AsSpan(_source)[row].Item1;
    public ref readonly LogMessage GetLogMessage(int row) => ref CollectionsMarshal.AsSpan(_source)[row].Item2;

    public void Dispose()
    {
        for (var i = 0; i < _source.Count; i++)
        {
            ref readonly var msg = ref GetLogMessage(i);
            if (msg.OuterData != null)
                _freeOuterData(msg.OuterData);
        }

        _source.Clear();
    }
}