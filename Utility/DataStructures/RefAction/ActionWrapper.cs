using System;
using System.Collections.Generic;
using System.Linq;

public class ActionWrapper : IInvokable
{
    private Action _a;

    public ActionWrapper(Action a)
    {
        _a = a;
    }

    public void Invoke()
    {
        _a.Invoke();
    }
}
public class ActionWrapper<T> : IInvokable<T>
{
    private Action<T> _a;

    public ActionWrapper(Action<T> a)
    {
        _a = a;
    }

    public void Invoke(T t)
    {
        _a.Invoke(t);
    }
}
