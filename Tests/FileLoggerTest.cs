using ConsoleDemo;
using NanoLog;
using NanoLog.File;

namespace Tests;

[TestClass]
public class FileLoggerTest
{
    [TestMethod]
    public void TestWriteAndRead()
    {
        var path = Path.Combine(Path.GetTempPath(), "logs");
        if (Directory.Exists(path))
            Directory.Delete(path, true);
        var logger = new FileLogger(path);

        var writer = new LogMessageWriter();
        writer.AppendLiteral("Hello");
        writer.AppendInt("score", 12345);
        writer.FinishWrite();

        //write one log
        var logEvent = new LogEvent(LogLevel.Debug, "category", "file.cs", "member", 128);
        logger.Log(ref logEvent, ref writer.Message);
        logger.Flush();
        Assert.AreEqual(1, logger.Files.Count);

        //read all logs
        var reader = new RecordReader();
        var logs = reader.ReadLogs(Path.Combine(path, logger.Files[0]));
        Assert.AreEqual(1, logs.Count);

        //cleanup
        Directory.Delete(path, true);
    }

    [TestMethod]
    public void MakeDemoLogFiles()
    {
        var path = Path.Combine(Path.GetTempPath(), "demologs");

        const string channel = "WebSocket";
        const int port = 12345;

        NanoLogger.Start(new NanoLoggerOptions().AddLogger(new FileLogger(path)));
        var log = new NanoLogger();
        log.Trace("Server start...");
        log.Debug($"Create channel: {channel}, port={port}");
        log.Info($"Init channel: {channel}, port={port}");
        log.Warn($"Channel port is bigger: {port}");
        log.Error($"Server stopped: {channel}");
        for (var i = 0; i < 1000; i++)
        {
            var point = new Point() { X = i, Y = i };
            log.Info($"Structured point: {point}");
        }

        NanoLogger.Stop();
    }
}