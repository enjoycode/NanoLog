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

    protected override void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        formatter.BeforeToken(TokenType.DateTime);
        formatter.WriteFormattable(value.ToLocalTime(), format);
        formatter.AfterToken(TokenType.DateTime);
    }
}