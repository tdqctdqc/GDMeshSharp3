using System;
using System.Collections.Generic;
using System.Linq;

public class CountHistory : History<int>
{
    protected CountHistory(Dictionary<int, int> byTick) : base(byTick)
    {
    }
    public static CountHistory Construct()
    {
        return new CountHistory(new Dictionary<int, int>());
    }
    public int GetLatestDelta()
    {
        if (_list.Count > 1)
        {
            return _list[_list.Count - 1].Value - _list[_list.Count - 2].Value;
        }
        return 0;
    }
}
