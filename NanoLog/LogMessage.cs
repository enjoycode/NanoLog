using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NanoLog;

[StructLayout(LayoutKind.Explicit, Size = LogMessageSize)]
public struct LogMessage
{
    [FieldOffset(0)] internal byte[]? ExtData;
    [FieldOffset(8)] private byte Data;

    private const int LogMessageSize = 256;
    internal const int InnerDataSize = LogMessageSize - 8;

    [UnscopedRef] internal ref byte DataPtr => ref Data;
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
    End = 0xFF
}