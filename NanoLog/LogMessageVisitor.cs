using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

public abstract unsafe class LogMessageVisitor
{
   

    //private LogMessage* _msg;

    public void Visit(ref LogMessage message)
    {
        //_msg = (LogMessage*)Unsafe.AsPointer(ref message);

        var pos = 0;
        var src = MemoryMarshal.CreateReadOnlySpan(ref message.DataPtr, LogMessage.InnerDataSize);
        while (true)
        {
            var tokenType = (TokenType)src[pos++];
            if (tokenType == TokenType.End)
                break;

            switch (tokenType)
            {
                case TokenType.Literal1:
                    
                    break;
                case TokenType.DateTime:
                    break;
                default:
                    break;
            }
        }
    }

    protected abstract void VisitLiteral(ReadOnlySpan<char> chars);

    protected abstract void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value);
}