using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class Logger
{
    private Data _data;
    public Dictionary<LogType, Dictionary<int, LogEntry>> Entries { get; private set; }
    private ConcurrentQueue<(LogType, string)> _queue;
    public Logger(Data data)
    {
        _data = data;
        Entries = new Dictionary<LogType, Dictionary<int, LogEntry>>();
        _queue = new ConcurrentQueue<(LogType, string)>();
        RunLoop();
    }

    private async void RunLoop()
    {
        await Task.Run(Loop);
    }
    private void Loop()
    {
        var tick = _data.Tick;
        while (true)
        {
            while (_queue.TryDequeue(out var res))
            {
                var entries = Entries.GetOrAdd(res.Item1, i => new Dictionary<int, LogEntry>());
                var entry = entries.GetOrAdd(tick, t => new LogEntry(t));
                entry.Logs.Add(res.Item2);
            }
        }
    }
    public void Log(string msg, LogType logType)
    {
        _queue.Enqueue((logType, msg));
    }

    public void RunAndLogTime(string name, LogType type, Action a)
    {
        var sw = new Stopwatch();
        sw.Start();
        a.Invoke();
        sw.Stop();
        var ms = sw.Elapsed.TotalMilliseconds;
        Log($"{name}: {ms} ms", type);
    }
}
