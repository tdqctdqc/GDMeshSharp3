using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.ObjectPool;

public class DefaultPool<T> : DefaultObjectPool<T> where T : class
{
    public DefaultPool(Func<T> constructor, int maximumRetained = 100_000) 
        : base(new DefaultNoticePolicy<T>(constructor), maximumRetained)
    {
    }
}
