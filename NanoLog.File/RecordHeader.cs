using System.Runtime.InteropServices;

namespace NanoLog.File;

internal enum RecordFlag : byte
{
    Empty = 0,
    First = 1,
    Middle = 2,
    Last = 3,
    Full = 4
}

[StructLayout(LayoutKind.Sequential, Size = HEADER_SIZE)]
internal struct RecordHeader
{
    public byte Reserved;
    public RecordFlag Flag;
    /// <summary>
    /// 包含头部的大小
    /// </summary>
    public ushort RecordSize;

    internal const int HEADER_SIZE = 4;
}