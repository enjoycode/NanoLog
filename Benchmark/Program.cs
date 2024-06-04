using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using NanoLog;

var benchConfig = DefaultConfig.Instance
    .AddJob(Job.Default.AsDefault());

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, benchConfig);

// NanoLogger.Start();
// var log = new NanoLogger();
// for (var i = 0; i < 100; i++)
// {
//     for (var j = 0; j < 10000; j++)
//     {
//         log.Info($"Hello World {DateTime.Now}");
//         Thread.Sleep(1);
//     }
// }
//
// NanoLogger.Stop();
// Console.WriteLine("Done.");