using System;
using Microsoft.Extensions.ObjectPool;

public class DefaultNoticePolicy<T> : PooledObjectPolicy<T> 
{
    private Func<T> _constructor;

    public DefaultNoticePolicy(Func<T> constructor) : base()
    {
        _constructor = constructor;
    }

    public override T Create()
    {
        return _constructor();
    }

    public override bool Return(T obj)
    {
        return true;
    }
}