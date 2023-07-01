
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GenReport
{
    public string Name { get; private set; }
    public List<string> Sections { get; private set; }
    public List<int> Times { get; private set; }
    public List<string> Feedbacks { get; private set; }
    private Stopwatch _sw;

    public GenReport(string name)
    {
        Name = name;
        Times = new List<int>();
        Sections = new List<string>();
        Feedbacks = new List<string>();
        _sw = new Stopwatch();
    }

    public void StartSection()
    {
        _sw.Reset();
        _sw.Start();
    }

    public void StopSection(string section, string feedback = "")
    {
        _sw.Stop();
        Sections.Add(section);
        Times.Add((int)_sw.Elapsed.TotalMilliseconds);
        Feedbacks.Add(feedback);
        _sw.Reset();
    }

    public string GetTimes()
    {
        var r = Name;
        for (var i = 0; i < Times.Count; i++)
        {
            r += $"\n \t \t {Sections[i]} Time: {Times[i]}ms";
        }

        r += $"\n \t \t Total Time: {Times.Sum()}ms";
        return r;
    }
}
