using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

internal unsafe ref struct LogMessageWriter
{
    public LogMessageWriter() {}
    
    private int _pos;
    private LogMessage _msg;

    [UnscopedRef] public ref LogMessage Message => ref _msg;

    /// <summary>
    /// 是否使用了扩展的数据存储
    /// </summary>
    private bool UseExt => _msg.ExtData != null;

    /// <summary>
    /// 当前缓存块剩余的可写入字节数
    /// </summary>
    private int Available => !UseExt ? LogMessage.InnerDataSize - _pos : _msg.ExtData!.Length - _pos;

    private Span<byte> WriteSpan => !UseExt
        ? MemoryMarshal.CreateSpan(ref _msg.DataPtr, LogMessage.InnerDataSize)
        : _msg.ExtData!.AsSpan();

    #region ====Write Data====

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
        var available = Available;
        if (available >= src.Length)
        {
            src.CopyTo(WriteSpan[_pos..]);
            _pos += src.Length;
        }
        else
        {
            if (available > 0)
                src[..available].CopyTo(WriteSpan[_pos..]);
            Grow(src.Length - available);
            src[available..].CopyTo(WriteSpan[_pos..]);
            _pos += src.Length - available;
        }
    }

    private void WriteByte(byte v)
    {
        if (Available >= 1)
        {
            WriteSpan[_pos++] = v;
        }
        else
        {
            Grow(1);
            WriteSpan[_pos++] = v;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteUShortValue(ushort v)
    {
        var ptr = (byte*)&v;
        WriteByte(ptr[0]);
        WriteByte(ptr[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteUIntValue(uint v)
    {
        var ptr = (byte*)&v;
        WriteByte(ptr[0]);
        WriteByte(ptr[1]);
        WriteByte(ptr[2]);
        WriteByte(ptr[3]);
    }

    /// <summary>
    /// 写入最长255字符的string
    /// </summary>
    private void WriteShortStringValue(string? shortString)
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
            WriteUShortValue((ushort)v.Length);
        }
        else
        {
            WriteByte((byte)TokenType.Literal4);
            WriteUIntValue((uint)v.Length);
        }

        var src = MemoryMarshal.AsBytes(v.AsSpan());
        Write(src);
    }

    public void AppendDateTime(string name, DateTime v, string? format)
    {
        WriteByte((byte)TokenType.DateTime);
        WriteShortStringValue(name);
        WriteShortStringValue(format);
        var src = new ReadOnlySpan<byte>((byte*)&v, 8);
        Write(src);
    }
}