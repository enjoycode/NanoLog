using System.Collections;
using System.Runtime.InteropServices;

namespace NanoLog;

public sealed class LogList(List<(LogEvent, LogMessage)> source, Action<byte[]> freeOuterData)
    : IDisposable
{
    public int Count => source.Count;
    public ref readonly LogEvent GetLogEvent(int row) => ref CollectionsMarshal.AsSpan(source)[row].Item1;
    public ref readonly LogMessage GetLogMessage(int row) => ref CollectionsMarshal.AsSpan(source)[row].Item2;

    public void Dispose()
    {
        for (var i = 0; i < source.Count; i++)
        {
            ref readonly var msg = ref GetLogMessage(i);
            if (msg.OuterData != null)
                freeOuterData(msg.OuterData);
        }

        source.Clear();
    }
}