using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

public abstract unsafe class LogMessageVisitor
{
    public void Visit(ref readonly LogMessage message)
    {
        var pos = 0;
        var src = MemoryMarshal.CreateReadOnlySpan(ref message.DataPtr, LogMessage.InnerDataSize);
        while (pos < src.Length)
        {
            var tokenType = (TokenType)src[pos++];
            if (tokenType == TokenType.End)
                break;

            switch (tokenType)
            {
                case TokenType.Literal1:
                {
                    EnsureAvailable(1, ref pos, ref src, message.ExtData);
                    var len = src[pos++] * 2;
                    EnsureAvailable(len, ref pos, ref src, message.ExtData);
                    VisitLiteral(MemoryMarshal.Cast<byte, char>(src.Slice(pos, len)));
                    pos += len;
                    break;
                }
                case TokenType.DateTime:
                {
                    EnsureAvailable(1, ref pos, ref src, message.ExtData);
                    var len = src[pos++] * 2;
                    
                    
                    break;
                }
                default:
                    break;
            }
        }
    }

    private static void EnsureAvailable(int required, ref int pos, ref ReadOnlySpan<byte> buf, byte[]? extData)
    {
        var isExt = extData != null &&
                    Unsafe.AsPointer(ref MemoryMarshal.GetReference(buf)) ==
                    Unsafe.AsPointer(ref MemoryMarshal.GetReference(extData.AsSpan()));
        var available = !isExt ? LogMessage.InnerDataSize - pos : extData!.Length - pos;
        if (available >= required)
            return;

        if (isExt || extData == null)
            throw new IndexOutOfRangeException();

        pos = 0;
        buf = extData.AsSpan();
    }

    protected abstract void VisitLiteral(ReadOnlySpan<char> chars);

    protected abstract void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value);
}