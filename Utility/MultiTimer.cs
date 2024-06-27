using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MultiTimer
{
    private Dictionary<string, ConcurrentBag<Stopwatch>> _sws;

    public MultiTimer()
    {
        _sws = new Dictionary<string, ConcurrentBag<Stopwatch>>();
    }

    public void AddName(string name)
    {
        _sws.Add(name, new ConcurrentBag<Stopwatch>());
    }
    
    public void RunAndTime(Action a, string name)
    {
        var sw = new Stopwatch();
        sw.Start();
        a.Invoke();
        sw.Stop();
        _sws[name].Add(sw);
    }
    public T RunAndTime<T>(Func<T> a, string name)
    {
        var sw = new Stopwatch();
        sw.Start();
        var res = a.Invoke();
        sw.Stop();
        _sws[name].Add(sw);
        return res;
    }
    public void AddTime(string name, Stopwatch sw)
    {
        _sws[name].Add(sw);
    }

    public void Print()
    {
        foreach (var (name, sws) in _sws)
        {
            var time = sws.Sum(sw => sw.Elapsed.Milliseconds);
            GD.Print($"{name}: {time}");
        }
    }
}