namespace NanoLog;

public sealed class ConsoleMessageVisitor(ConsoleFormatter formatter) : LogMessageVisitor
{
    protected override void VisitLiteral(ReadOnlySpan<char> chars)
    {
        formatter.WriteChars(chars);
    }

    protected override void VisitNull(ReadOnlySpan<char> name)
    {
        formatter.BeforeToken(TokenType.Null);
        formatter.WriteChars("NULL");
        formatter.AfterToken(TokenType.Null);
    }

    protected override void VisitBool(ReadOnlySpan<char> name, bool value)
    {
        formatter.BeforeToken(TokenType.Null);
        formatter.WriteChars(value ? "True" : "False");
        formatter.AfterToken(TokenType.Null);
    }

    protected override void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        formatter.BeforeToken(TokenType.DateTime);
        formatter.WriteFormattable(value.ToLocalTime(), format);
        formatter.AfterToken(TokenType.DateTime);
    }

    protected override void VisitString(ReadOnlySpan<char> name, ReadOnlySpan<char> value)
    {
        formatter.BeforeToken(TokenType.String4);
        formatter.WriteChars(value);
        formatter.AfterToken(TokenType.String4);
    }
}