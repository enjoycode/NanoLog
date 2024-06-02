
using NanoLog;

NanoLogger.Start();

var log = new NanoLogger();
log.Debug($"Hello World! {DateTime.Now}, 你好世界!");

NanoLogger.Stop();
Console.WriteLine("Stopped.");