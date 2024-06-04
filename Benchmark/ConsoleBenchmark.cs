using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using NanoLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Benchmark;

[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
[ThreadingDiagnoser]
public class ConsoleBenchmark
{
    private readonly ILogger _msLogger;
    private readonly NanoLogger _nanoLogger;
    private readonly DateTime _now = DateTime.Now;

    public ConsoleBenchmark()
    {
        var msFactory = LoggerFactory.Create(builder => builder.AddSystemdConsole());
        _msLogger = msFactory.CreateLogger("Program");

        NanoLogger.Start();
        _nanoLogger = new NanoLogger("Program");
    }

    [Benchmark]
    public void NanoLog()
    {
        _nanoLogger.Info($"Hello World {_now}");
    }

    [Benchmark(Baseline = true)]
    public void MsLog()
    {
        _msLogger.LogInformation("Hello World {Now}", _now);
    }

    [Benchmark]
    public void MsLogCodeGen()
    {
        DelegateLog.Log(_msLogger, _now);
    }
}

// BenchmarkDotNet v0.13.12, macOS Sonoma 14.5 (23F79) [Darwin 23.5.0]
// Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
//     .NET SDK 8.0.301
//     [Host]     : .NET 8.0.6 (8.0.624.26715), Arm64 RyuJIT AdvSIMD
// Job-QQHNWQ : .NET 8.0.6 (8.0.624.26715), Arm64 RyuJIT AdvSIMD
//
//     IterationCount=1  LaunchCount=1  WarmupCount=0
//
//     | Method    | Mean       | Error | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
//     |---------- |-----------:|------:|------:|-------:|-------:|----------:|------------:|
//     | NanoLog   |   786.6 ns |    NA |  0.20 | 0.1297 | 0.0439 |     816 B |        3.09 |
//     | MsLog     | 3,937.6 ns |    NA |  1.00 | 0.0381 | 0.0153 |     264 B |        1.00 |
//     | MsFastLog | 3,975.2 ns |    NA |  1.01 | 0.0305 | 0.0153 |     208 B |        0.79 |