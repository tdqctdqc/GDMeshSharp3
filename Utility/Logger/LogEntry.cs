
using System.Collections.Generic;

public class LogEntry
{
    public int Tick { get; private set; }
    public List<string> Logs { get; private set; }

    public LogEntry(int tick)
    {
        Tick = tick;
        Logs = new List<string>();
    }
}