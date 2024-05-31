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
        var charSpan = MemoryMarshal.Cast<byte, char>(span[2..]) ;
        Assert.IsTrue(literal.AsSpan().SequenceEqual(charSpan));
    }
}