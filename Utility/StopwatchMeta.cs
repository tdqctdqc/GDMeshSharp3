
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public static class StopwatchMeta
{
    public static Dictionary<string, Stopwatch> Dic = new Dictionary<string, Stopwatch>();
    private static Dictionary<string, List<double>> _times = new Dictionary<string, List<double>>();
    public static void Start(string tag)
    {
        var sw = new Stopwatch();
        Dic.Add(tag, sw);
        sw.Start();
    }
    public static void TryStart(string tag)
    {
        if(Dic.ContainsKey(tag) == false) Start(tag);
        else
        {
            TryStop(tag);
            Dic[tag].Start();
        }
    }
    public static void TryStop(string tag)
    {
        if(Dic.ContainsKey(tag))
        {
            var sw =  Dic[tag];
            sw.Stop();
            var t = sw.Elapsed.TotalMilliseconds;
            if(_times.ContainsKey(tag) == false) _times.Add(tag, new List<double>());
            _times[tag].Add(t);
            sw.Reset();
        }
    }

    // public static void StopAndDump(string tag)
    // {
    //     
    // }

    public static void DumpAll()
    {
        foreach (var kvp in Dic)
        {
            var tag = kvp.Key;
            if (_times.ContainsKey(tag))
            {
                var t = _times[tag].Sum();
                GD.Print(kvp.Key + " took " + t + "ms");
                // _times[tag].ForEach(t => 
                //     GD.Print(kvp.Key + " took " + t + "ms")
                // );
            }
        }
        Dic.Clear();
    }
}
