using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace NanoLog.File;

internal sealed class RecordWriter
{
    public RecordWriter(FileLogger logger)
    {
        _logger = logger;
    }

    private readonly FileLogger _logger;
    private readonly byte[] _buffer = new byte[FileLogger.PAGE_SIZE];
    internal SafeFileHandle? FileHandle;
    private int _pagePos;
    private int _writePos;
    private int _headerPos;

    private int PageAvailable => FileLogger.PAGE_SIZE - _pagePos;

    private ref RecordHeader HeaderPtr => ref Unsafe.As<byte, RecordHeader>(
        ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(_buffer), _headerPos));

    // |--------------------------File----------------------------|
    // |-------Page--------|-------Page--------|-------Page-------|
    // |-R1-|--R2--|-R3-|P-|-------R4.1--------|-R4.2-|

    // |-----------------Record---------------|
    // |-Header-|--EventData--|--MessageData--|
    // | 2Byte  |   n Bytes   |     nBytes    |

    internal unsafe void Write(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        if (_pagePos + _writePos >= FileLogger.FILE_SIZE)
        {
            MoveToNextFile(logEvent.Time);
        }
        else if (PageAvailable <= RecordHeader.HEADER_SIZE)
        {
            if (_pagePos + _writePos + PageAvailable >= FileLogger.FILE_SIZE)
                MoveToNextFile(logEvent.Time);
            else
                MoveToNextPage();
        }

        if (FileHandle == null) return;

        StartRecord(true);

        //DebugLevel
        _buffer[_writePos++] = (byte)logEvent.Level;
        //Timestamp
        var ticks = logEvent.Time.Ticks;
        WriteToBuffer(new ReadOnlySpan<byte>(&ticks, 8));
        //Category & File & Member
        WriteShortString(logEvent.Category);
        WriteShortString(logEvent.File);
        WriteShortString(logEvent.Member);
        //Line
        var line = logEvent.Line;
        WriteToBuffer(new ReadOnlySpan<byte>(&line, 4));
        
    }

    private void WriteShortString(string value)
    {
        var len = Math.Min(value.Length, byte.MaxValue);
        WriteToBuffer((byte)len);
        if (len > 0)
            WriteToBuffer(MemoryMarshal.AsBytes(value.AsSpan(0, len)));
    }

    private void WriteToBuffer(byte value)
    {
        if (PageAvailable >= 1)
        {
            _buffer.AsSpan()[_writePos++] = value;
            return;
        }

        FinishRecord(false);
        MoveToNextPage();
        StartRecord(false);
        _buffer.AsSpan()[_writePos++] = value;
    }

    private void WriteToBuffer(ReadOnlySpan<byte> src)
    {
        while (true)
        {
            var available = PageAvailable;
            if (available >= src.Length)
            {
                src.CopyTo(_buffer.AsSpan(_writePos));
                _writePos += src.Length;
                return;
            }

            src[..available].CopyTo(_buffer.AsSpan(_writePos));
            _writePos += available;
            FinishRecord(false);
            MoveToNextPage();
            StartRecord(false);
            src = src[available..];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void StartRecord(bool isFirst)
    {
        _headerPos = _writePos;
        HeaderPtr.Flag = isFirst ? RecordFlag.First : RecordFlag.Middle;
        _writePos += RecordHeader.HEADER_SIZE;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FinishRecord(bool isLast)
    {
        HeaderPtr.RecordSize = (ushort)(_writePos - _headerPos);

        if (isLast)
            HeaderPtr.Flag = HeaderPtr.Flag == RecordFlag.First ? RecordFlag.Full : RecordFlag.Last;
    }

    private void MoveToNextPage()
    {
        RandomAccess.Write(FileHandle!, _buffer, _pagePos);
        _pagePos += FileLogger.PAGE_SIZE;
        _writePos = 0;
        ClearBuffer();
    }

    private void MoveToNextFile(DateTime start)
    {
        FileHandle?.Close();
        FileHandle = _logger.CreateFile(_logger.CurrentSeq + 1, start);

        ClearBuffer();
        _pagePos = 0;
        _writePos = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearBuffer() => _buffer.AsSpan().Clear();
}