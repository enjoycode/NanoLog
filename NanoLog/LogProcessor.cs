using System.Buffers;
using System.Threading.Channels;

namespace NanoLog;

internal sealed class LogProcessor
{
    public LogProcessor(int capacity = 10000)
    {
        _channel = Channel.CreateBounded<(LogEvent, LogMessage)>(new BoundedChannelOptions(capacity)
            { SingleReader = true });
        // _channel = Channel.CreateUnbounded<(LogEvent, LogMessage)>(
        //     new UnboundedChannelOptions() { SingleReader = true });
        _thread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Name = "NanoLogProcessor"
        };
        _thread.Start();
    }

    private readonly Channel<(LogEvent, LogMessage)> _channel; //TODO: custom channel for use ref to avoid some memcopy
    private readonly Thread _thread;

    private async void ProcessLogQueue()
    {
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var job))
            {
                var loggers = NanoLogger.Options.Loggers;
                foreach (var logger in loggers)
                {
                    logger.Log(ref job.Item1, ref job.Item2);
                }

                if (job.Item2.ExtData != null)
                    ArrayPool<byte>.Shared.Return(job.Item2.ExtData);
            }
        }
    }

    public void Enqueue(ref readonly LogEvent logEvent, ref readonly LogMessage message)
    {
        if (_channel.Writer.TryWrite((logEvent, message)))
            return;

        EnqueueAsync(logEvent, message);
    }

    private async void EnqueueAsync(LogEvent logEvent, LogMessage message)
    {
        try
        {
            await _channel.Writer.WriteAsync((logEvent, message));
        }
        catch (Exception)
        {
            // do nothing
        }
    }

    public void Stop()
    {
        _channel.Writer.Complete();
        _thread.Join();
    }
}