
using NanoLog;

NanoLogger.Start();

var log = new NanoLogger();
log.Trace("Trace message");
log.Trace($"Trace {DateTime.Now}");
log.Debug($"Debug {DateTime.Now}, 你好世界!");
log.Info($"Info {DateTime.Now}");
log.Warn($"这是警告");
log.Error($"发生异常");

NanoLogger.Stop();
// Console.WriteLine("Stopped.");