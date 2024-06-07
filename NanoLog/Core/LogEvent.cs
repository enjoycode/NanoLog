using System.Runtime.InteropServices;

namespace NanoLog;

[StructLayout(LayoutKind.Sequential)]
public readonly struct LogEvent()
{
    public LogEvent(LogLevel level, string category, string file, string member, int line) : this()
    {
        Time = DateTime.UtcNow;
        Level = level;
        Category = category;
        File = file;
        Member = member;
        Line = line;
    }

    public LogEvent(DateTime time, LogLevel level, string category, string file, string member, int line) : this()
    {
        Time = time;
        Level = level;
        Category = category;
        File = file;
        Member = member;
        Line = line;
    }

    public readonly LogLevel Level;
    public readonly int Line;
    public readonly DateTime Time;
    public readonly string Category;
    public readonly string File;
    public readonly string Member;
}