using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Logger
{
    public Dictionary<LogType, List<string>> Logs { get; private set; }

    public Logger()
    {
        Logs = new Dictionary<LogType, List<string>>();
    }
    
    public void Log(string msg, LogType logType)
    {
        Logs.AddOrUpdate(logType, msg);
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
