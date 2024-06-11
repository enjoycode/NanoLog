using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

public abstract unsafe class LogMessageVisitor
{
    private byte* _innerDataPtr;
    private byte[]? _extData;
    private int _pos;
    private bool _useExt;
    private bool _firstMember;
    private int _depth;

    protected bool IsLogValueMember => _depth > 0;

    protected bool IsFirstMember => _firstMember;

    #region =====Read Data====

    private ReadOnlySpan<byte> ReadSpan =>
        _useExt ? _extData.AsSpan() : new ReadOnlySpan<byte>(_innerDataPtr, LogMessage.InnerDataSize);

    private void EnsureAvailable(int required)
    {
        var available = !_useExt ? LogMessage.InnerDataSize - _pos : _extData!.Length - _pos;
        if (available >= required)
            return;

        if (_useExt || _extData == null)
            throw new IndexOutOfRangeException();

        _useExt = true;
        _pos = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> Read(int bytesCount)
    {
        EnsureAvailable(bytesCount);
        var res = ReadSpan.Slice(_pos, bytesCount);
        _pos += bytesCount;
        return res;
    }

    private void ReadTo(Span<byte> dest)
    {
        EnsureAvailable(dest.Length);
        ReadSpan.Slice(_pos, dest.Length).CopyTo(dest);
        _pos += dest.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadByte()
    {
        EnsureAvailable(1);
        return ReadSpan[_pos++];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<char> ReadChars(int charsCount) =>
        MemoryMarshal.Cast<byte, char>(Read(charsCount * 2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<char> ReadShortString()
    {
        var charsCount = (int)ReadByte();
        return charsCount <= 0 ? ReadOnlySpan<char>.Empty : ReadChars(charsCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort ReadUShort()
    {
        ushort res = 0;
        ReadTo(new Span<byte>(&res, 2));
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ReadInt()
    {
        int res = 0;
        ReadTo(new Span<byte>(&res, 4));
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint ReadUInt()
    {
        uint res = 0;
        ReadTo(new Span<byte>(&res, 4));
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long ReadLong()
    {
        var res = 0L;
        ReadTo(new Span<byte>(&res, 8));
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double ReadDouble()
    {
        var res = 0.0d;
        ReadTo(new Span<byte>(&res, 8));
        return res;
    }

    /// <summary>
    /// Read DateTime(UTC) value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DateTime ReadDateTime()
    {
        var ticks = ReadLong();
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Guid ReadGuid()
    {
        var bytes = stackalloc byte[16];
        ReadTo(new Span<byte>(bytes, 16));
        return new Guid(new ReadOnlySpan<byte>(bytes, 16));
    }

    #endregion

    public void Visit(ref readonly LogMessage message)
    {
        _depth = 0;
        _pos = 0;
        _useExt = false;
        _innerDataPtr = (byte*)Unsafe.AsPointer(ref message.InnerDataPtr);
        _extData = message.OuterData;

        while (true)
        {
            if (!_useExt && _pos == LogMessage.InnerDataSize && _extData == null)
                break;

            var tokenType = (TokenType)ReadByte();
            switch (tokenType)
            {
                case TokenType.Literal1:
                    if (VisitLiteral(ReadChars(ReadByte())))
                        return;
                    break;
                case TokenType.Literal2:
                    if (VisitLiteral(ReadChars(ReadUShort())))
                        return;
                    break;
                case TokenType.Literal4:
                    if (VisitLiteral(ReadChars((int)ReadUInt())))
                        return;
                    break;
                case TokenType.Null:
                    if (VisitNull(ReadShortString()))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.BoolTrue:
                    if (VisitBool(ReadShortString(), true))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.BoolFalse:
                    if (VisitBool(ReadShortString(), false))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.Char:
                    if (VisitChar(ReadShortString(), (char)ReadUShort()))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.Int:
                    if (VisitInt(ReadShortString(), ReadShortString(), ReadInt()))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.Double:
                    if (VisitDouble(ReadShortString(), ReadShortString(), ReadDouble()))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.DateTime:
                    if (VisitDateTime(ReadShortString(), ReadShortString(), ReadDateTime()))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.Guid:
                    if (VisitGuid(ReadShortString(), ReadGuid()))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.String1:
                    if (VisitString(ReadShortString(), ReadChars(ReadByte())))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.String2:
                    if (VisitString(ReadShortString(), ReadChars(ReadUShort())))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.String4:
                    if (VisitString(ReadShortString(), ReadChars((int)ReadUInt())))
                        return;
                    _firstMember = false;
                    break;
                case TokenType.LogValue:
                    if (BeginVisitLogValue(ReadShortString()))
                        return;
                    _firstMember = true;
                    _depth++;
                    break;
                case TokenType.LogValueEndMembers:
                    _depth--;
                    _firstMember = false;
                    if (EndVisitLogValue())
                        return;
                    break;
                case TokenType.End:
                    return;
                default:
                    return;
            }
        }
    }

    protected abstract bool VisitLiteral(ReadOnlySpan<char> chars);

    protected abstract bool VisitNull(ReadOnlySpan<char> name);

    protected abstract bool VisitBool(ReadOnlySpan<char> name, bool value);

    protected abstract bool VisitChar(ReadOnlySpan<char> name, char value);

    protected abstract bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value);

    protected abstract bool VisitDouble(ReadOnlySpan<char> name, ReadOnlySpan<char> format, double value);

    protected abstract bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value);

    protected abstract bool VisitGuid(ReadOnlySpan<char> name, Guid value);

    protected abstract bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value);

    protected abstract bool BeginVisitLogValue(ReadOnlySpan<char> name);

    protected abstract bool EndVisitLogValue();
}