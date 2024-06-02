namespace NanoLog;

public sealed class ConsoleMessageVisitor(ConsoleFormatter formatter) : LogMessageVisitor
{
    protected override void VisitLiteral(ReadOnlySpan<char> chars)
    {
        formatter.WriteChars(chars);
    }

    protected override void VisitDateTime(ReadOnlySpan<char> name, ReadOnlySpan<char> format, DateTime value)
    {
        formatter.BeforeToken(TokenType.DateTime);
        formatter.WriteFormattable(value.ToLocalTime(), format);
        formatter.AfterToken(TokenType.DateTime);
    }
}