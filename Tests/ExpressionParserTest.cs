using System.Linq.Expressions;
using NanoLog;
using NanoLog.File.Viewer;

namespace Tests;

[TestClass]
public class ExpressionParserTest
{
    private static bool Eval(string exp, LogDataParameter para, bool falseWhenError = true)
    {
        try
        {
            return ExpressionParser.ParseAndCompile(exp)(para);
        }
        catch (Exception ex)
        {
            if (falseWhenError)
            {
                Console.WriteLine($"Type: {ex.GetType().Name} Msg: {ex.Message}");
                return false;
            }

            throw;
        }
    }

    [TestMethod]
    public void TestParse()
    {
        var writer = new LogMessageWriter();
        writer.AppendLiteral("Hello");
        writer.AppendInt("index", 12345);
        writer.FinishWrite();
        var logEvent = new LogEvent(LogLevel.Debug, "app", "aa.cs", "say", 32);

        var list = new List<(LogEvent, LogMessage)> { (logEvent, writer.Message) };
        var para = new LogDataParameter(list, 0);

        Assert.IsTrue(Eval("e.Level >= LogLevel.Debug", para));
        Assert.IsFalse(Eval("e[\"value\"]==12345", para)); //不存在的属性
        Assert.IsTrue(Eval("e.Level >= LogLevel.Debug && e[\"index\"]==12345", para, false));
        Assert.IsTrue(Eval("e.Level > LogLevel.Debug || e[\"index\"]==12345", para, false));
    }

    [TestMethod]
    public void LinqExpressionCompareEnum()
    {
        var a = Expression.Constant(LogLevel.Debug, typeof(LogLevel));
        var b = Expression.Constant(LogLevel.Warning, typeof(LogLevel));

        var ca = Expression.Convert(a, typeof(int));
        var cb = Expression.Convert(b, typeof(int));
        var compare = Expression.MakeBinary(ExpressionType.GreaterThanOrEqual, ca, cb);
    }
}