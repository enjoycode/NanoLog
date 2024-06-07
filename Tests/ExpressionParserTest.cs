using System.Linq.Expressions;
using NanoLog;
using NanoLog.File.Viewer;

namespace Tests;

[TestClass]
public class ExpressionParserTest
{
    [TestMethod]
    public void TestParse()
    {
        var exp = "e.Level >= LogLevel.Debug";
        var parser = ExpressionParser.ParseAndCompile(exp);
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