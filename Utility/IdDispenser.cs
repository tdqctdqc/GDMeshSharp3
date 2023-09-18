using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public class IdDispenser : Entity
{
    public int Index { get; private set; }

    public static IdDispenser Create(GenWriteKey key)
    {
        var d = new IdDispenser(0, -1);
        key.Create(d);
        return d;
    }
    [SerializationConstructor] private IdDispenser(int index, int id) : base(id)
    {
        Index = index;
    }
    public int TakeId()
    {
        Index++;
        if (Index == int.MaxValue) throw new Exception("Max Ids reached");
        int id = Index;
        return id;
    }

    public void SetMin(int taken)
    {
        Index = Mathf.Max(taken, Index);
    }
}