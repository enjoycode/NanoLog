﻿using NanoLog;

NanoLogger.Start();

DateTime? nullable = null;
const bool boolValue = true;

var log = new NanoLogger();
log.Trace("Trace message");
log.Trace($"Trace {DateTime.Now}");
log.Debug($"Debug {DateTime.Now:yyyy-MM-dd hh:mm:ss}, 你好世界!");
log.Info($"Info {DateTime.Now}");
log.Warn($"这是警告: {boolValue}");
log.Error($"发生异常: {nullable}");

NanoLogger.Stop();
// Console.WriteLine("Stopped.");