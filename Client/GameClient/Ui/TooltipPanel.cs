using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

public partial class TooltipPanel : PanelContainer
{
    public VBoxContainer Container;
    private static float _margin = 20f;
    private static float _detailTime = .25f;
    private object _element;
    private VBoxContainer _fast, _slow;
    
    private TimerAction _detailAction;
    public TooltipPanel()
    {
        var margin = new MarginContainer();
        AddChild(margin);
        Container = new VBoxContainer();
        margin.AddChild(Container);
        _detailAction = new TimerAction(.25f, .25f,
            () => { }, true);
    }
    public override void _Process(double delta)
    {
        _detailAction?.Process(delta);
    }
    public void Setup<TElement>
        (VBoxContainer fast, VBoxContainer slow, 
            TElement element, Data data)
    {
        Size = Vector2.Zero;
        Clear();
        _fast = fast;
        _slow = slow;
        _element = element;
        AddFastEntries(data);
        _detailAction.ResetTimer();
        _detailAction.SetAction(() => AddSlowEntries(data));
        Size = Vector2.Zero;
    }

    private void Clear()
    {
        Size = Vector2.Zero;
        Container.ClearChildren();
        _fast = null;
        _slow = null;
        _detailAction.SetAction(() => {});
    }

    
    private void AddFastEntries(Data data)
    {
        Size = Vector2.Zero;
        if(_fast is not null) Container.AddChild(_fast);
    }
    private void AddSlowEntries(Data data)
    {
        Size = Vector2.Zero;
        if(_slow is not null) Container.AddChild(_slow);
    }
    public void Move(Vector2 globalPos)
    {
        GlobalPosition = globalPos;
    }
}