using Godot;
using System;
using System.Collections.Generic;

public class LazyDic<TElement, TValue>
{
    private Func<TElement, TValue> _func;
    private Dictionary<TElement, TValue> _dic;

    public LazyDic(Func<TElement, TValue> func)
    {
        _func = func;
        _dic = new Dictionary<TElement, TValue>();
    }

    public TValue Get(TElement el)
    {
        if (_dic.ContainsKey(el) == false)
        {
            _dic.Add(el, _func(el));
        }

        return _dic[el];
    }
}
