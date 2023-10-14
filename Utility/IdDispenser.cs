using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public class IdDispenser : Entity
{
    public int Index => _index;
    private int _index;
    public static IdDispenser Create(GenWriteKey key)
    {
        var d = new IdDispenser(0, 0);
        key.Create(d);
        d.Id = d.TakeId();
        return d;
    }
    [SerializationConstructor] private IdDispenser(int index, int id) : base(id)
    {
        _index = index;
    }
    public int TakeId()
    {
        if (_index == int.MaxValue) throw new Exception("Max Ids reached");
        int id = System.Threading.Interlocked.Increment(ref _index);
        return id;
    }

    public void SetMin(int taken)
    {
        _index = Mathf.Max(taken, _index);
    }
}