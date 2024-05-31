
using NanoLog;

NanoLogger.Start();

var log = new NanoLogger();
log.Debug($"Hello World! {DateTime.Now}");

NanoLogger.Stop();