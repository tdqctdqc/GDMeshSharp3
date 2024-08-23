
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class TimerTreeNode
{
    public string Name { get; private set; }
    public Stopwatch Stopwatch { get; private set; }
    public List<TimerTreeNode> Children { get; private set; }

    public TimerTreeNode(string name)
    {
        Name = name;
        Stopwatch = new Stopwatch();
    }

    public void Start()
    {
        Stopwatch.Start();
    }

    public void Stop()
    {
        Stopwatch.Stop();
    }

    public TimerTreeNode AddChildAndStart(string name)
    {
        var c = new TimerTreeNode(name);
        if (Children is null) Children = new List<TimerTreeNode>();
        Children.Add(c);
        c.Start();
        return c;
    }

    public TimerTreeNode GetOrAddChildAndStart(string name)
    {
        if (Children is not null 
            && Children.FirstOrDefault(c => c.Name == name) is TimerTreeNode t)
        {
            t.Start();
            return t;
        }

        return AddChildAndStart(name);
    }
    

    public void RunChild(string name, Action action)
    {
        var c = GetOrAddChildAndStart(name);
        action.Invoke();
        c.Stop();
    }
    
    public T RunChild<T>(string name, Func<T> func)
    {
        var c = GetOrAddChildAndStart(name);
        var t = func.Invoke();
        c.Stop();
        return t;
    }

    public string GetString()
    {
        return $"{Name}: {Stopwatch.Elapsed.TotalMilliseconds} ms";
    }

    public Control GetNode()
    {
        return Write(0, false);
    }

    private VBoxContainer Write(int depth, bool childrenVisible)
    {
        var outer = new VBoxContainer();
        var inner = new HBoxContainer();
        outer.AddChild(inner);
        var t = "";
        for (var i = 0; i < depth; i++)
        {
            t += "......";
        }

        t += GetString();
        if (Children is not null)
        {
            var children = new VBoxContainer();
            children.Visible = childrenVisible;
            outer.AddChild(children);
            Button button = null;
            button = inner.AddButton(childrenVisible ? "Hide" : "Show",
                () =>
                {
                    children.Visible = !children.Visible;
                    button.Text = children.Visible ? "Hide" : "Show";
                });
            foreach (var c in Children)
            {
                children.AddChild(c.Write(depth + 1, true));
            }
        }
        else
        {
            inner.AddButton("......", () => { });
        }
        inner.CreateLabelAsChild(t);


        return outer;
    }
}