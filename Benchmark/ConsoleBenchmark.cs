using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using NanoLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Benchmark;

[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 200, invocationCount:10000)]
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
// Job-RFJVHW : .NET 8.0.6 (8.0.624.26715), Arm64 RyuJIT AdvSIMD
//
//     InvocationCount=10000  IterationCount=200  LaunchCount=1
// WarmupCount=1
//
//     | Method       | Mean       | Error    | StdDev    | Ratio | RatioSD | Completed Work Items | Lock Contentions | Allocated | Alloc Ratio |
//     |------------- |-----------:|---------:|----------:|------:|--------:|---------------------:|-----------------:|----------:|------------:|
//     | NanoLog      |   154.6 ns |  0.91 ns |   3.48 ns |  0.04 |    0.00 |                    - |                - |         - |        0.00 |
//     | MsLog        | 3,922.2 ns | 49.13 ns | 202.60 ns |  1.00 |    0.00 |                    - |           0.0004 |     264 B |        1.00 |
//     | MsLogCodeGen | 4,079.3 ns | 52.49 ns | 218.77 ns |  1.04 |    0.07 |                    - |           0.0010 |     208 B |        0.79 |