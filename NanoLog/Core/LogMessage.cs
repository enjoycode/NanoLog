using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NanoLog;

[StructLayout(LayoutKind.Explicit, Size = LogMessageSize)]
public struct LogMessage
{
    [FieldOffset(0)] internal byte[]? OuterData;
    [FieldOffset(8)] private int Length;
    [FieldOffset(12)] private byte InnerData;

    private const int LogMessageSize = 256;
    internal const int InnerDataSize = LogMessageSize - 8 - 4;

    public int InnerDataLength
    {
        get => (Length >> 24) & 0xFF;
        internal set => Length = (value << 24) | (Length & 0xFFFFFF);
    }

    public int OuterDataLength
    {
        get => Length & 0xFFFFFF;
        internal set => Length |= (value & 0xFFFFFF);
    }

    [UnscopedRef] public ref byte InnerDataPtr => ref InnerData;

    public ReadOnlySpan<byte> InnerDataForRead => MemoryMarshal.CreateReadOnlySpan(ref InnerData, InnerDataLength);

    public ReadOnlySpan<byte> OuterDataForRead => OuterData == null ? default : OuterData.AsSpan(0, OuterDataLength);
}

public enum TokenType : byte
{
    None,
    Literal1,
    Literal2,
    Literal4,
    Null,
    BoolTrue,
    BoolFalse,
    Byte,
    Char,
    Short,
    UShort,
    Int,
    UInt,
    Long,
    ULong,
    Float,
    Double,
    Decimal,
    DateTime,
    Guid,
    String1,
    String2,
    String4,
    LogValue,
    LogValueEndMembers,
    End = 0xFF
}