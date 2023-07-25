using System;
using System.Collections.Generic;
using System.Linq;

public class RefFunc<TArg, TRes>
{
    private Func<TArg, TRes> _func;

    public TRes Invoke(TArg arg)
    {
        return _func(arg);
    }
    public void Subscribe(RefFunc<TArg, TRes> a)
    {
        _func += a.Invoke;
    }
    public void Subscribe(Func<TArg, TRes> a)
    {
        _func += a.Invoke;
    }
    public void Unsubscribe(RefFunc<TArg, TRes> a)
    {
        _func -= a.Invoke;
    }
    public void Unsubscribe(ref Func<TArg, TRes> a)
    {
        _func -= a.Invoke;        
    }
}
