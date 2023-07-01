
using System;
using System.Collections.Generic;
using Godot;

public class RefAction
{
    private Action _action;
    private HashSet<RefAction> _subscribingTo;
    private HashSet<RefAction> _refSubscribers;

    public RefAction()
    {
    }
    public void Invoke()
    {
        _action?.Invoke();
        if (_refSubscribers != null)
        {
            foreach (var refSubscriber in _refSubscribers)
            {
                refSubscriber.Invoke();
            }
        }
    }

    public void Clear()
    {
        _action = () => { };
        EndSubscriptions();
    }
    public void Subscribe(Action a)
    {
        _action += a;
    }
    public void Subscribe(RefAction a)
    {
        // _action += a.Invoke;
        if (_refSubscribers == null) _refSubscribers = new HashSet<RefAction>();
        _refSubscribers.Add(a);
        if (a._subscribingTo == null) a._subscribingTo = new HashSet<RefAction>();
        a._subscribingTo.Add(this);
    }
    public void Unsubscribe(Action a)
    {
        _action -= a;
    }
    public void Unsubscribe(RefAction a)
    {
        _refSubscribers.Remove(a);
    }
    public void EndSubscriptions()
    {
        if (_subscribingTo == null) return;
        foreach (var refAction in _subscribingTo)
        {
            refAction.Unsubscribe(this);
        }
        _subscribingTo.Clear();
    }
}
public class RefAction<TArg>
{
    public RefAction Blank { get; private set; }
    public HashSet<RefAction<TArg>> _refSubscribers;
    private Action<TArg> _action;
    private HashSet<RefAction<TArg>> _subscribingTo;
    public RefAction()
    {
        Blank = new RefAction();
        _action += t => Blank.Invoke();
    }
    
    public void Invoke(TArg t)
    {
        _action?.Invoke(t);
        if (_refSubscribers != null)
        {
            foreach (var refSubscriber in _refSubscribers)
            {
                refSubscriber.Invoke(t);
            }
        }
        Blank.Invoke();
    }
    public void Subscribe(RefAction a)
    {
        Blank.Subscribe(a);
    }
    public void Subscribe(RefAction<TArg> a)
    {
        if (_refSubscribers == null) _refSubscribers = new HashSet<RefAction<TArg>>();
        _refSubscribers.Add(a);
        
        if (a._subscribingTo == null) a._subscribingTo = new HashSet<RefAction<TArg>>();
        a._subscribingTo.Add(this);
    }
    public void Subscribe(Action<TArg> a)
    {
        _action += a.Invoke;
    }
    public void Unsubscribe(RefAction<TArg> a)
    {
        _refSubscribers.Remove(a);
    }
    public void Unsubscribe(ref Action<TArg> a)
    {
        _action -= a.Invoke;        
    }
    public void Unubscribe(RefAction a)
    {
        Blank.Unsubscribe(a);
    }
    public void EndSubscriptions()
    {
        if (_subscribingTo == null) return;
        foreach (var refAction in _subscribingTo)
        {
            refAction.Unsubscribe(this);
        }
        _subscribingTo.Clear();
    }

    public void Clear()
    {
        _refSubscribers = null;
        _action = null;
        Blank = new RefAction();
    }
}