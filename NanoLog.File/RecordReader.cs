using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace NanoLog.File;

public sealed class RecordReader
{
    private SafeFileHandle _fileHandle = null!;
    private byte[]? _buffer;
    private int _pagePos;
    private int _readPos;

    public unsafe LogList ReadLogs(string logfile)
    {
        if (!System.IO.File.Exists(logfile))
            throw new ArgumentException("File not exists.");

        _fileHandle = System.IO.File.OpenHandle(logfile, options: FileOptions.SequentialScan);

        try
        {
            var fileSize = (int)RandomAccess.GetLength(_fileHandle);
            var records = new List<(LogEvent, LogMessage)>(100000);

            _buffer = new byte[FileLogger.PAGE_SIZE];
            _pagePos = 0;
            _readPos = 0;

            while (_pagePos < fileSize)
            {
                RandomAccess.Read(_fileHandle, _buffer, _pagePos);

                while (_readPos < FileLogger.PAGE_SIZE)
                {
                    ref var headerPtr = ref Unsafe.As<byte, RecordHeader>(
                        ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), _readPos));
                    _readPos += RecordHeader.HEADER_SIZE;
                    if (_readPos >= FileLogger.PAGE_SIZE)
                        break; //met padding
                    if (headerPtr.Flag == RecordFlag.Empty)
                        return new LogList(records, d => ArrayPool<byte>.Shared.Return(d)); //met ending
                    if (headerPtr.Reserved != RecordHeader.RESERVED_DATA)
                        break; //met wrong data, skip this page

                    //Level
                    var level = (LogLevel)ReadByte();
                    //Timestamp
                    var ticks = 0L;
                    ReadTo(new Span<byte>(&ticks, 8));
                    //Category & File & Member
                    var category = ReadShortString();
                    var sourceFile = ReadShortString();
                    var member = ReadShortString();
                    //Line
                    var line = 0;
                    ReadTo(new Span<byte>(&line, 4));

                    //Message length
                    var logMessage = new LogMessage();
                    var msgLen = 0;
                    ReadTo(new Span<byte>(&msgLen, 4));
                    logMessage.InnerDataLength = (msgLen >> 24) & 0xFF;
                    logMessage.OuterDataLength = msgLen & 0xFFFFFF;

                    ReadTo(MemoryMarshal.CreateSpan(ref logMessage.InnerDataPtr, logMessage.InnerDataLength));
                    if (logMessage.OuterDataLength > 0)
                    {
                        logMessage.OuterData = ArrayPool<byte>.Shared.Rent(logMessage.OuterDataLength);
                        ReadTo(logMessage.OuterData.AsSpan());
                    }

                    records.Add((
                        new LogEvent(new DateTime(ticks, DateTimeKind.Utc), level, category, sourceFile, member, line),
                        logMessage));
                }

                _pagePos += FileLogger.PAGE_SIZE;
                _readPos = 0;
            }

            return new LogList(records, d => ArrayPool<byte>.Shared.Return(d));
        }
        finally
        {
            _fileHandle.Close();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ReadShortString()
    {
        var len = ReadByte();
        return len == 0
            ? string.Empty
            : string.Create(len, 0, (dest, _) => ReadTo(MemoryMarshal.AsBytes(dest)));
    }

    private byte ReadByte()
    {
        var available = FileLogger.PAGE_SIZE - _readPos;
        if (available >= 1)
        {
            return _buffer![_readPos++];
        }

        MoveToNextPage();
        return _buffer![_readPos++];
    }

    private void ReadTo(Span<byte> dest)
    {
        while (true)
        {
            var available = FileLogger.PAGE_SIZE - _readPos;
            if (available >= dest.Length)
            {
                _buffer.AsSpan(_readPos, dest.Length).CopyTo(dest);
                _readPos += dest.Length;
                return;
            }

            _buffer.AsSpan(_readPos, available).CopyTo(dest);
            _readPos += available;
            MoveToNextPage();
            dest = dest[available..];
        }
    }

    private void MoveToNextPage()
    {
        _pagePos += FileLogger.PAGE_SIZE;
        RandomAccess.Read(_fileHandle, _buffer, _pagePos);
        _readPos = RecordHeader.HEADER_SIZE;
    }
}