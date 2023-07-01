using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SubscribedNodeToken : Node 
{
    private Node _t;
    private Action _update;

    public static SubscribedNodeToken Construct(Node node, Action update, params RefAction[] triggers)
    {
        var s = new SubscribedNodeToken();
        s.Setup(node, update, triggers);
        return s;
    }
    protected void Setup(Node t, Action update, params RefAction[] triggers)
    {
        _t = t;
        if (_t == null) throw new Exception();
        _t.AddChild(this);
        
        foreach (var trigger in triggers)
        {
            AddTrigger(trigger);
        }
        _update = update;
    }

    public void TriggerUpdate()
    {
        _update.Invoke();
    }
    public void AddTrigger(RefAction trigger)
    {
        trigger.Subscribe(TriggerUpdate);
        TreeExiting += () => trigger.Unsubscribe(TriggerUpdate);
    }
}
