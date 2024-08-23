
using System.Collections.Generic;
using Godot;

public class LogEntry
{
    public int Tick { get; private set; }
    public List<Node> Logs { get; private set; }

    public LogEntry(int tick)
    {
        Tick = tick;
        Logs = new List<Node>();
    }
}