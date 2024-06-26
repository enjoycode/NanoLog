using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

public unsafe ref struct LogMessageWriter
{
    public LogMessageWriter() { }

    private int _pos;
    private LogMessage _msg;

    [UnscopedRef] public ref LogMessage Message => ref _msg;

    /// <summary>
    /// 是否使用了扩展的数据存储
    /// </summary>
    private bool UseOuter => _msg.OuterData != null;

    #region ====Write Data====

    private Span<byte> WriteSpan => !UseOuter
        ? MemoryMarshal.CreateSpan(ref _msg.InnerDataPtr, LogMessage.InnerDataSize)
        : _msg.OuterData!.AsSpan();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureAvailable(int required)
    {
        var available = !UseOuter ? LogMessage.InnerDataSize - _pos : _msg.OuterData!.Length - _pos;
        if (available >= required)
            return;
        Grow(required);
    }

    private void Grow(int required)
    {
        if (!UseOuter)
        {
            _msg.InnerDataLength = _pos;
            _msg.OuterData = ArrayPool<byte>.Shared.Rent(Math.Max(256, required));
            _pos = 0;
        }
        else
        {
            //TODO:考虑自定义ArrayPool，因为multi thread rent, one thread return.
            var newExtData = ArrayPool<byte>.Shared.Rent(Math.Max(required, (int)(_msg.OuterData!.Length * 1.5)));
            _msg.OuterData.AsSpan().CopyTo(newExtData.AsSpan());
            ArrayPool<byte>.Shared.Return(_msg.OuterData);
            _msg.OuterData = newExtData;
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
    private void WriteShort(short v)
    {
        EnsureAvailable(2);
        var ptr = (byte*)&v;
        var dest = WriteSpan[_pos..];
        dest[0] = ptr[0];
        dest[1] = ptr[1];
        _pos += 2;
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

    internal void AppendLiteral(string v)
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
    internal void AppendNull(string name)
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

    public void AppendShort(string name, short? v)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.Short);
            WriteShortString(name);
            WriteShort((short)v);
            return;
        }

        AppendNull(name);
    }

    public void AppendInt(string name, int? v, string? format = null)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.Int);
            WriteShortString(name);
            WriteShortString(format);
            var value = v.Value;
            Write(new ReadOnlySpan<byte>((byte*)&value, 4));
            return;
        }

        AppendNull(name);
    }

    public void AppendULong(string name, ulong? v, string? format = null)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.ULong);
            WriteShortString(name);
            WriteShortString(format);
            var value = v.Value;
            Write(new ReadOnlySpan<byte>((byte*)&value, 8));
            return;
        }

        AppendNull(name);
    }

    public void AppendDouble(string name, double? v, string? format = null)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.Double);
            WriteShortString(name);
            WriteShortString(format);
            var value = v.Value;
            Write(new ReadOnlySpan<byte>((byte*)&value, 8));
            return;
        }

        AppendNull(name);
    }

    public void AppendDateTime(string name, DateTime? v, string? format = null)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.DateTime);
            WriteShortString(name);
            WriteShortString(format);
            var ticks = v.Value.ToUniversalTime().Ticks;
            Write(new ReadOnlySpan<byte>((byte*)&ticks, 8));
            return;
        }

        AppendNull(name);
    }

    public void AppendGuid(string name, Guid? v)
    {
        if (v.HasValue)
        {
            WriteByte((byte)TokenType.Guid);
            WriteShortString(name);
            var bytes = stackalloc byte[16];
            v.Value.TryWriteBytes(new Span<byte>(bytes, 16));
            Write(new ReadOnlySpan<byte>(bytes, 16));
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

    public void AppendLogValue<T>(string name, in T v) where T : ILogValue
    {
        if (!v.IsScalar)
        {
            WriteByte((byte)TokenType.LogValue);
            WriteShortString(name);
            v.AppendMembers(ref this, name);
            WriteByte((byte)TokenType.LogValueEndMembers);
            return;
        }

        v.AppendMembers(ref this, name);
    }

    public void AppendStructLogValue<T>(string name, ref T v) where T : struct, ILogValue
    {
        if (!v.IsScalar)
        {
            WriteByte((byte)TokenType.LogValue);
            WriteShortString(name);
            v.AppendMembers(ref this, name);
            WriteByte((byte)TokenType.LogValueEndMembers);
            return;
        }

        v.AppendMembers(ref this, name);
    }

    internal void FinishWrite()
    {
        if (!UseOuter && _pos == LogMessage.InnerDataSize)
        {
            _msg.InnerDataLength = LogMessage.InnerDataSize;
            return; //正好全部使用内置数据块
        }

        WriteSpan[_pos++] = (byte)TokenType.End;
        if (UseOuter)
            _msg.OuterDataLength = _pos;
        else
            _msg.InnerDataLength = _pos;
    }
}