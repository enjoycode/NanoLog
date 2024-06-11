using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NanoLog;

public sealed class ConsoleMessageVisitor(ConsoleFormatter formatter) : LogMessageVisitor
{
    protected override bool VisitLiteral(ReadOnlySpan<char> chars)
    {
        formatter.WriteChars(chars);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void TryWriteMemberName(ReadOnlySpan<char> name)
    {
        if (IsLogValueMember)
        {
            if (!IsFirstMember)
                formatter.WriteChars(", ");

            formatter.WriteChars(name);
            formatter.WriteChars(": ");
        }
    }

    protected override bool VisitNull(ReadOnlySpan<char> name)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.Null);
        formatter.WriteChars("NULL");
        formatter.AfterToken(TokenType.Null);
        return false;
    }

    protected override bool VisitBool(ReadOnlySpan<char> name, bool value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.Null);
        formatter.WriteChars(value ? "True" : "False");
        formatter.AfterToken(TokenType.Null);
        return false;
    }

    protected override bool VisitChar(ReadOnlySpan<char> name, char value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.Char);
        formatter.WriteByte((byte)'\'');
        formatter.WriteChars(MemoryMarshal.CreateReadOnlySpan(ref value, 1));
        formatter.WriteByte((byte)'\'');
        formatter.AfterToken(TokenType.Char);
        return false;
    }

    protected override bool VisitInt(ReadOnlySpan<char> name, ReadOnlySpan<char> format, int value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.Int);
        formatter.WriteFormattable(value, format);
        formatter.AfterToken(TokenType.Int);
        return false;
    }

    protected override bool VisitDouble(ReadOnlySpan<char> name, ReadOnlySpan<char> format, double value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.Double);
        formatter.WriteFormattable(value, format);
        formatter.AfterToken(TokenType.Double);
        return false;
    }

    protected override bool VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.DateTime);
        formatter.WriteFormattable(value.ToLocalTime(), format);
        formatter.AfterToken(TokenType.DateTime);
        return false;
    }

    protected override bool VisitGuid(ReadOnlySpan<char> name, Guid value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.Guid);
        formatter.WriteFormattable(value);
        formatter.AfterToken(TokenType.Guid);
        return false;
    }

    protected override bool VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        TryWriteMemberName(name);

        formatter.BeforeToken(TokenType.String4);
        formatter.WriteByte((byte)'"');
        formatter.WriteChars(value);
        formatter.WriteByte((byte)'"');
        formatter.AfterToken(TokenType.String4);
        return false;
    }

    protected override bool BeginVisitLogValue(ReadOnlySpan<char> name)
    {
        //formatter.BeforeToken(TokenType.LogValue);
        formatter.WriteByte((byte)'{');
        return false;
    }

    protected override bool EndVisitLogValue()
    {
        formatter.WriteByte((byte)'}');
        //formatter.BeforeToken(TokenType.LogValue);
        return false;
    }
}