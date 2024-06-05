using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Win32.SafeHandles;

namespace NanoLog.File;

public sealed class FileLogger : ILogger
{
    public FileLogger(string folder)
    {
        _folder = folder;
        _recordWriter = new RecordWriter(this);

        ulong lastSeq = 0;
        if (!Directory.Exists(_folder))
        {
            Directory.CreateDirectory(_folder);
        }
        else
        {
            _files.AddRange(Directory.EnumerateFiles(_folder));
            //按流水号排序
            _files.Sort((a, b) => ParseSeqFromName(a).CompareTo(ParseSeqFromName(b)));
            lastSeq = CurrentSeq;
        }

        //暂始终新建文件
        _recordWriter.FileHandle = CreateFile(lastSeq + 1, DateTime.Now);
    }

    private readonly List<string> _files = [];
    private readonly string _folder;
    private readonly RecordWriter _recordWriter;

    internal const int FILE_SIZE = 32 * 1024 * 1024;
    internal const int PAGE_SIZE = 4 * 1024;

    internal ulong CurrentSeq => _files.Count == 0 ? 0 : ParseSeqFromName(_files[^1]);

    public void Log(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        _recordWriter.Write(in logEvent, in message);
    }

    public void Flush()
    {
        _recordWriter.WriteBufferToFile();
    }

    #region ====Files====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string MakeFileName(ulong seq, DateTime start) => $"{seq:x16}-{start:yyMMdd-hh}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ParseSeqFromName(string fileName) =>
        ulong.Parse(fileName.AsSpan(0, 16), NumberStyles.HexNumber);

    internal SafeFileHandle? CreateFile(ulong seq, DateTime start)
    {
        var fileName = MakeFileName(seq, start);
        var fullPath = Path.Combine(_folder, fileName);
        try
        {
            var fileHandler = System.IO.File.OpenHandle(fullPath, FileMode.CreateNew,
                access: FileAccess.Write,
                options: FileOptions.None,
                preallocationSize: FILE_SIZE);
            _files.Add(fileName);
            return fileHandler;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    #endregion
}