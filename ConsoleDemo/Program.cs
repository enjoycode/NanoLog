﻿using ConsoleDemo;
using NanoLog;

NanoLogger.Start();

DateTime? nullable = null;
const bool boolValue = true;
const char charValue = 'C';
const int intValue1 = 12345;
const int intValue2 = 0xABCDEF;
const string stringValue = "你好世界";
var point = new Point { X = 123, Y = 456 };
var person = new Person { Name = "Rick", Birthday = new DateTime(1977, 3, 1), Phone = "13861838709" };

var log = new NanoLogger();

log.Trace("Trace message");
log.Trace($"Trace {DateTime.Now}, {intValue1}, 0x{intValue2:X}");
log.Debug($"Debug {DateTime.Now:yyyy-MM-dd hh:mm:ss}, 你好世界!");
log.Info($"Info {point}, {person}, {charValue}");
log.Warn($"这是警告: {boolValue}");
log.Error($"发生异常: {nullable}, Msg={stringValue}");

NanoLogger.Stop();
Console.WriteLine("Done.");