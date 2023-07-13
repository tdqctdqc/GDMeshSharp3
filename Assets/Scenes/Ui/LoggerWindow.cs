using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class LoggerWindow : ClosableWindow
{
    private Container _container;
    private float _timer = 0f;
    private float _updatePeriod = .5f;
    private Dictionary<LogType, int> _num;
    private Dictionary<LogType, Node> _innerContainers;
    
    public static LoggerWindow Get()
    {
        return SceneManager.Instance<LoggerWindow>();
    }

    private LoggerWindow()
    {
        _num = new Dictionary<LogType, int>();
        _innerContainers = new Dictionary<LogType, Node>();
        Hide();
        Size = Vector2I.One * 500;
        AboutToPopup += Draw;
    }
    public override void _Ready()
    {
        _container = (Container) FindChild("Container");
        // _container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
    }

    public override void _Process(double delta)
    {
        if (Visible)
        {
            _timer += (float)delta;
            if (_timer > _updatePeriod)
            {
                _timer = 0f;
                foreach (var kvp in _num)
                {
                    var logType = kvp.Key;
                    var oldNum = kvp.Value;
                    if (_innerContainers.ContainsKey(logType) == false) continue;
                    var innerContainer = _innerContainers[logType];
                    var msgs = Game.I.Logger.Logs[logType];
                    var newMsgs = msgs.GetRange(oldNum, msgs.Count - oldNum);
                    for (var i = 0; i < newMsgs.Count; i++)
                    {
                        innerContainer.AddChild(NodeExt.CreateLabel(newMsgs[i]));
                    }
                }
                foreach (var kvp in Game.I.Logger.Logs)
                {
                    _num[kvp.Key] = kvp.Value.Count;
                }
            }
        }
    }
    private void Draw()
    {
        _timer = 0f;
        _container.ClearChildren();
        _num.Clear();
        _innerContainers.Clear();
        foreach (var kvp in Game.I.Logger.Logs)
        {
            AddTab(kvp.Key, kvp.Value);
        }
    }

    private void AddTab(LogType lt, List<string> msgs)
    {
        var name = Enum.GetName(typeof(LogType), lt);
        var scroll = new ScrollContainer();
        _container.AddChild(scroll);
        scroll.Name = name;

        var vbox = new VBoxContainer();
        // vbox.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _innerContainers.Add(lt, vbox);
        _num.Add(lt, msgs.Count);
        scroll.AddChild(vbox);
        for (var i = 0; i < msgs.Count; i++)
        {
            vbox.AddChild(NodeExt.CreateLabel(msgs[i]));
        }
    }
}