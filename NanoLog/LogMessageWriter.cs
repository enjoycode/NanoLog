using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

internal unsafe ref struct LogMessageWriter
{
    public LogMessageWriter()
    {
    }

    private int _pos;
    private LogMessage _msg;

    [UnscopedRef] public ref LogMessage Message => ref _msg;

    /// <summary>
    /// 是否使用了扩展的数据存储
    /// </summary>
    private bool UseExt => _msg.ExtData != null;

    #region ====Write Data====

    private Span<byte> WriteSpan => !UseExt
        ? MemoryMarshal.CreateSpan(ref _msg.DataPtr, LogMessage.InnerDataSize)
        : _msg.ExtData!.AsSpan();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureAvailable(int required)
    {
        var available = !UseExt ? LogMessage.InnerDataSize - _pos : _msg.ExtData!.Length - _pos;
        if (available >= required)
            return;
        Grow(required);
    }

    private void Grow(int required)
    {
        if (!UseExt)
        {
            _msg.ExtData = ArrayPool<byte>.Shared.Rent(Math.Max(256, required));
            _pos = 0;
        }
        else
        {
            //TODO:考虑自定义ArrayPool，因为multi thread rent, one thread return.
            var newExtData = ArrayPool<byte>.Shared.Rent(Math.Max(required, (int)(_msg.ExtData!.Length * 1.5)));
            _msg.ExtData.AsSpan().CopyTo(newExtData.AsSpan());
            ArrayPool<byte>.Shared.Return(_msg.ExtData);
            _msg.ExtData = newExtData;
        }
    }

    private void Write(ReadOnlySpan<byte> src)
    {
        EnsureAvailable(src.Length);
        src.CopyTo(WriteSpan[_pos..]);
        _pos += src.Length;
    }

    private void WriteByte(byte v)
    {
        EnsureAvailable(1);
        WriteSpan[_pos++] = v;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteUShort(ushort v)
    {
        EnsureAvailable(2);
        var ptr = (byte*)&v;
        var dest = WriteSpan[_pos..];
        dest[0] = ptr[0];
        dest[1] = ptr[1];
        _pos += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteUInt(uint v)
    {
        EnsureAvailable(4);
        var ptr = (byte*)&v;
        var dest = WriteSpan[_pos..];
        dest[0] = ptr[0];
        dest[1] = ptr[1];
        dest[2] = ptr[2];
        dest[3] = ptr[3];
        _pos += 4;
    }

    /// <summary>
    /// 写入最长255字符的string
    /// </summary>
    private void WriteShortString(string? shortString)
    {
        if (string.IsNullOrEmpty(shortString))
        {
            WriteByte(0);
            return;
        }

        var len = Math.Min(shortString.Length, 255);
        WriteByte((byte)len);
        Write(MemoryMarshal.AsBytes(shortString.AsSpan(0, len)));
    }

    #endregion

    public void AppendLiteral(string v)
    {
        if (v.Length < byte.MaxValue)
        {
            WriteByte((byte)TokenType.Literal1);
            WriteByte((byte)v.Length);
        }
        else if (v.Length < ushort.MaxValue)
        {
            WriteByte((byte)TokenType.Literal2);
            WriteUShort((ushort)v.Length);
        }
        else
        {
            WriteByte((byte)TokenType.Literal4);
            WriteUInt((uint)v.Length);
        }

        Write(MemoryMarshal.AsBytes(v.AsSpan()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendNull(string name)
    {
        WriteByte((byte)TokenType.Null);
        WriteShortString(name);
    }

    public void AppendBool(string name, bool? v)
    {
        if (v.HasValue)
        {
            WriteByte(v.Value ? (byte)TokenType.BoolTrue : (byte)TokenType.BoolFalse);
            WriteShortString(name);
            return;
        }

        AppendNull(name);
    }

    public void AppendChar(string name, char? v)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.Char);
            WriteShortString(name);
            WriteUShort((ushort)v);
            return;
        }

        AppendNull(name);
    }

    public void AppendDateTime(string name, DateTime? v, string? format)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.DateTime);
            WriteShortString(name);
            WriteShortString(format);
            var ticks = v.Value.ToUniversalTime().Ticks;
            var src = new ReadOnlySpan<byte>((byte*)&ticks, 8);
            Write(src);
            return;
        }

        AppendNull(name);
    }

    public void AppendString(string name, string? v)
    {
        if (v != null)
        {
            if (v.Length < byte.MaxValue)
            {
                WriteByte((byte)TokenType.String1);
                WriteShortString(name);
                WriteByte((byte)v.Length);
            }
            else if (v.Length < ushort.MaxValue)
            {
                WriteByte((byte)TokenType.String2);
                WriteShortString(name);
                WriteUShort((ushort)v.Length);
            }
            else
            {
                WriteByte((byte)TokenType.String4);
                WriteShortString(name);
                WriteUInt((uint)v.Length);
            }

            Write(MemoryMarshal.AsBytes(v.AsSpan()));
            return;
        }

        AppendNull(name);
    }

    public void FinishWrite()
    {
        if (!UseExt && _pos == LogMessage.InnerDataSize)
            return; //正好全部使用内置数据块
        WriteSpan[_pos++] = (byte)TokenType.End;
    }
}