using System.Text.Unicode;

namespace NanoLog;

public abstract class ConsoleFormatter(Stream output)
{
    private readonly byte[] _buffer = new byte[256];
    private int _pos;

    public abstract void Output(ref readonly LogEvent logEvent, ref readonly LogMessage message);

    protected internal virtual void BeforeToken(TokenType tokenType)
    {
    }

    protected internal virtual void AfterToken(TokenType tokenType)
    {
    }

    protected internal void WriteByte(byte b)
    {
        _buffer[_pos++] = b;
        if (_pos == _buffer.Length)
        {
            output.Write(_buffer);
            _pos = 0;
        }
    }

    protected internal void WriteBytes(ReadOnlySpan<byte> s)
    {
        while (true)
        {
            var dst = _buffer.AsSpan(_pos);
            if (dst.Length >= s.Length)
            {
                s.CopyTo(dst);
                _pos += s.Length;
                return;
            }

            s.Slice(0, dst.Length).CopyTo(dst);
            output.Write(_buffer, 0, _pos);
            _pos = 0;
            s = s.Slice(dst.Length);
        }
    }

    protected internal void WriteChars(ReadOnlySpan<char> s)
    {
        while (true)
        {
            var dst = _buffer.AsSpan(_pos);
            Utf8.FromUtf16(s, dst, out var charsRead, out var bytesWritten);
            _pos += bytesWritten;

            if (charsRead != s.Length)
            {
                output.Write(_buffer, 0, _pos);
                _pos = 0;
                s = s.Slice(charsRead);
                continue;
            }

            break;
        }
    }

    protected internal void WriteFormattable<T>(T v, ReadOnlySpan<char> format = default)
        where T : struct, IUtf8SpanFormattable
    {
        while (true)
        {
            var dst = _buffer.AsSpan(_pos);
            if (v.TryFormat(dst, out var bytesWritten, format, null))
            {
                _pos += bytesWritten;
                return;
            }

            if (_pos == 0)
            {
                var s = v.ToString(); //v.ToString(format.IsEmpty ? null : new string(format));
                WriteChars(s);
                return;
            }

            output.Write(_buffer, 0, _pos);
            _pos = 0;
        }
    }

    protected void FlushBuffer()
    {
        if (_pos == 0) return;
        output.Write(_buffer, 0, _pos);
        _pos = 0;
        output.Flush();
    }
}