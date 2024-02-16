
using System;

public class IdDispenser
{
    public int Index => _index;
    private int _index;
    public IdDispenser(int index)
    {
        _index = index;
    }

    private readonly object _lock = new object();
    public int TakeId()
    {
        if (_index == int.MaxValue) throw new Exception("Max Ids reached");
        lock (_lock)
        {
            System.Threading.Interlocked.Increment(ref _index);
            return _index;
        }
    }
}