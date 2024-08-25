using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class LoggerWindow : Window
{
    private Container _container;
    private float _timer = 0f;
    private float _updatePeriod = .5f;
    private Dictionary<LogType, int> _num;
    private Dictionary<LogType, Node> _innerContainers;
    private Data _data;
    public static void Open(Data data)
    {
        var w = SceneManager.Instance<LoggerWindow>();
        w._data = data;
        Game.I.Client.WindowHolder.OpenWindowFullSize(w);
    }

    private LoggerWindow()
    {
        this.MakeFreeable();
        _num = new Dictionary<LogType, int>();
        _innerContainers = new Dictionary<LogType, Node>();
        Hide();
        Size = Vector2I.One * 500;
        AboutToPopup += Draw;
    }
    public override void _Ready()
    {
        _container = (Container) FindChild("Container");
        _container.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
    }
    private void Draw()
    {
        _timer = 0f;
        _container.UnparentChildren();
        _num.Clear();
        _innerContainers.Clear();
        foreach (var kvp in _data.Logger.Entries)
        {
            AddTab(kvp.Key, kvp.Value);
        }
    }

    private void AddTab(LogType lt, Dictionary<int, LogEntry> entries)
    {
        var name = Enum.GetName(typeof(LogType), lt);
        var vbox = _container.MakeScrollChild<VBoxContainer>(out var scroll);
        scroll.Name = name;
        _innerContainers.Add(lt, vbox);
        _num.Add(lt, entries.Count);

        var entriesInOrder = entries.Values
            .OrderBy(v => v.Tick).ToList();
        for (var i = 0; i < entriesInOrder.Count; i++)
        {
            AddLogEntry(vbox, entriesInOrder[i]);
        }
    }

    private void AddLogEntry(Node parent, LogEntry entry)
    {
        var vbox = new VBoxContainer();
        var inner = new VBoxContainer();
        inner.Visible = false;
        var button = vbox.AddButton("Tick " + entry.Tick,
            () =>
            {
                inner.Visible = inner.Visible == false;
                foreach (var child in inner.GetChildren())
                {
                    inner.RemoveChild(child);
                }
                if (inner.Visible)
                {
                    for (var i = 0; i < entry.Logs.Count; i++)
                    {
                        inner.AddChild(entry.Logs[i]);
                    }
                }
            }
        );
        vbox.AddChild(inner);
        parent.AddChild(vbox);
    }
}