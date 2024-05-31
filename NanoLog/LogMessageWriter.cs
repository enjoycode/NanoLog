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
        var dest = WriteSpan;
        dest[0] = ptr[0];
        dest[1] = ptr[1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteUInt(uint v)
    {
        EnsureAvailable(4);
        var ptr = (byte*)&v;
        var dest = WriteSpan;
        dest[0] = ptr[0];
        dest[1] = ptr[1];
        dest[2] = ptr[2];
        dest[3] = ptr[3];
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
        var src = MemoryMarshal.AsBytes(shortString.AsSpan(0, len));
        Write(src);
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

        var src = MemoryMarshal.AsBytes(v.AsSpan());
        Write(src);
    }

    public void AppendDateTime(string name, DateTime v, string? format)
    {
        WriteByte((byte)TokenType.DateTime);
        WriteShortString(name);
        WriteShortString(format);
        var ticks = v.ToUniversalTime().Ticks;
        var src = new ReadOnlySpan<byte>((byte*)&ticks, 8);
        Write(src);
    }

    public void FinishWrite()
    {
        if (!UseExt && _pos == LogMessage.InnerDataSize)
            return; //正好全部使用内置数据块
        WriteSpan[_pos++] = (byte)TokenType.End;
    }
}