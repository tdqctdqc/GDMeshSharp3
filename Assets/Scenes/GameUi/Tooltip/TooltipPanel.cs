using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Thread = System.Threading.Thread;

public partial class TooltipPanel : Panel
{
    private VBoxContainer _container;
    private static float _margin = 20f;
    private static float _detailTime = .25f;
    private ITooltipInstance _instance;
    private TimerAction _detailAction;
    public TooltipPanel()
    {
        _container = new VBoxContainer();
        AddChild(_container);
        _detailAction = new TimerAction(.25f, .25f, () => { }, true);
    }
    public override void _Process(double delta)
    {
        _detailAction?.Process(delta);
    }
    public void Setup(ITooltipInstance instance, Data data)
    {
        Clear();
        _instance = instance;
        AddFastEntries(data);
        _detailAction.ResetTimer();
        _detailAction.SetAction(() => AddSlowEntries(data));
    }

    private void Clear()
    {
        _container.ClearChildren();
        _instance = null;
    }

    private void Resize()
    {
        var contChildren = _container.GetChildList();
        var totY = contChildren.Sum(c => ((Control) c).Size.Y);
        var topX = contChildren.Max(c => ((Control) c).Size.X);
        _container.Size = new Vector2(topX, totY);
        SetSize(_container.Size + Vector2.One * _margin);
        _container.Position = Vector2.One * _margin * .5f; 
    }
    private void AddFastEntries(Data data)
    {
        var oldSize = _container.Size;
        var entries = _instance.Template.GetFastEntries(_instance.Element, data);
        entries.ForEach(e => _container.AddChild(e));
        Resize();

        Task.Run(() =>
        {
            Thread.Sleep(1);
        });
    }
    private void AddSlowEntries(Data data)
    {
        if (_instance == null) return;
        var entries = _instance.Template.GetSlowEntries(_instance.Element, data);
        entries.ForEach(e => _container.AddChild(e));
        Resize();

        Task.Run(() =>
        {
            Thread.Sleep(1);
        });
    }
    public void Move(Vector2 globalPos)
    {
        GlobalPosition = globalPos;
    }
}