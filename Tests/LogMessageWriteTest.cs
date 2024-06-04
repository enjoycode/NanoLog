using System.Runtime.InteropServices;
using NanoLog;

namespace Tests;

[TestClass]
public class LogMessageWriteTest
{
    [TestMethod]
    public void TestMethod1()
    {
        const string literal = "Hello";
        var writer = new LogMessageWriter();
        writer.AppendLiteral(literal);

        var span = MemoryMarshal.CreateSpan(ref writer.Message.DataPtr, 2 + literal.Length * 2);
        Console.WriteLine(Convert.ToHexString(span));
        Assert.IsTrue(span[0] == (byte)TokenType.Literal1);
        Assert.IsTrue(span[1] == literal.Length);
        var charSpan = MemoryMarshal.Cast<byte, char>(span[2..]);
        Assert.IsTrue(literal.AsSpan().SequenceEqual(charSpan));
    }

    [TestMethod]
    public void TestVisitor()
    {
        var writer = new LogMessageWriter();
        writer.AppendLiteral("Hello");
        writer.AppendDateTime("Now", DateTime.Now, "yyyy-MM-dd hh:mm:ss.fff");
        writer.AppendLiteral("中国");
        writer.FinishWrite();

        var dump = new LogMessageDump();
        dump.Visit(ref writer.Message);
    }
}