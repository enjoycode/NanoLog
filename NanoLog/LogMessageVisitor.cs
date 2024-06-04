using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

public abstract unsafe class LogMessageVisitor
{
    private byte* _innerDataPtr;
    private byte[]? _extData;
    private int _pos;
    private bool _useExt;

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

    /// <summary>
    /// Read DateTime(UTC) value
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DateTime ReadDateTime()
    {
        var ticks = ReadLong();
        return new DateTime(ticks, DateTimeKind.Utc);
    }

    #endregion

    public void Visit(ref readonly LogMessage message)
    {
        _pos = 0;
        _useExt = false;
        _innerDataPtr = (byte*)Unsafe.AsPointer(ref message.DataPtr);
        _extData = message.ExtData;

        while (true)
        {
            if (!_useExt && _pos == LogMessage.InnerDataSize && _extData == null)
                break;

            var tokenType = (TokenType)ReadByte();
            switch (tokenType)
            {
                case TokenType.Literal1:
                    VisitLiteral(ReadChars(ReadByte()));
                    break;
                case TokenType.Literal2:
                    VisitLiteral(ReadChars(ReadUShort()));
                    break;
                case TokenType.Literal4:
                    VisitLiteral(ReadChars((int)ReadUInt()));
                    break;
                case TokenType.Null:
                    VisitNull(ReadShortString());
                    break;
                case TokenType.BoolTrue:
                    VisitBool(ReadShortString(), true);
                    break;
                case TokenType.BoolFalse:
                    VisitBool(ReadShortString(), false);
                    break;
                case TokenType.Char:
                    VisitChar(ReadShortString(), (char)ReadUShort());
                    break;
                case TokenType.Int:
                    VisitInt(ReadShortString(), ReadShortString(), ReadInt());
                    break;
                case TokenType.DateTime:
                    VisitDateTime(ReadShortString(), ReadShortString(), ReadDateTime());
                    break;
                case TokenType.String1:
                    VisitString(ReadShortString(), ReadChars(ReadByte()));
                    break;
                case TokenType.String2:
                    VisitString(ReadShortString(), ReadChars(ReadUShort()));
                    break;
                case TokenType.String4:
                    VisitString(ReadShortString(), ReadChars((int)ReadUInt()));
                    break;
                case TokenType.End:
                    return;
                default:
                    return;
            }
        }
    }

    protected abstract void VisitLiteral(ReadOnlySpan<char> chars);

    protected abstract void VisitNull(ReadOnlySpan<char> name);

    protected abstract void VisitBool(ReadOnlySpan<char> name, bool value);

    protected abstract void VisitChar(ReadOnlySpan<char> name, char value);

    protected abstract void VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value);

    protected abstract void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value);

    protected abstract void VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value);
}