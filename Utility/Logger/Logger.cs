using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class Logger
{
    public Dictionary<LogType, List<string>> Logs { get; private set; }
    private ConcurrentQueue<(LogType, string)> _queue;
    public Logger()
    {
        Logs = new Dictionary<LogType, List<string>>();
        _queue = new ConcurrentQueue<(LogType, string)>();
        RunLoop();
    }

    private async void RunLoop()
    {
        await Task.Run(Loop);
    }
    private void Loop()
    {
        while (true)
        {
            while (_queue.TryDequeue(out var res))
            {
                Logs.AddOrUpdate(res.Item1, res.Item2);
            }
        }
    }
    public void Log(string msg, LogType logType)
    {
        // Logs.AddOrUpdate(logType, msg);
        _queue.Enqueue((logType, msg));
    }

    public void RunAndLogTime(Action a, string name, LogType type)
    {
        var sw = new Stopwatch();
        sw.Start();
        a.Invoke();
        sw.Stop();
        Logs.AddOrUpdate(type, $"{name} time {sw.Elapsed.TotalMilliseconds}");
    }
}
