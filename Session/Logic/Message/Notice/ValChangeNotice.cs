using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public abstract class ValChangeNotice
{
    public Entity Entity { get; private set; }
    protected ValChangeNotice(Entity entity)
    {
        Entity = entity;
    }

    public abstract void Clear();
}

public class ValChangeNotice<TProperty> : ValChangeNotice
{
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }

    public ValChangeNotice(Entity entity, TProperty newVal, TProperty oldVal) 
        : base(entity)
    {
        NewVal = newVal;
        OldVal = oldVal;
    }
    public override void Clear()
    {
        NewVal = default;
        OldVal = default;
    }
}