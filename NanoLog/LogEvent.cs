using System.Runtime.InteropServices;

namespace NanoLog;

[StructLayout(LayoutKind.Sequential)]
public readonly struct LogEvent(LogLevel level, string category, string file, string member, int line)
{
    public readonly LogLevel Level = level;
    public readonly int Line = line;
    public readonly DateTime Time = DateTime.UtcNow;
    public readonly string Category = category;
    public readonly string File = file;
    public readonly string Member = member;
}