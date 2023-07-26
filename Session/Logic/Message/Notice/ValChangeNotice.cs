using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public class ValChangeNotice<TEntity, TProperty> where TEntity : Entity
{
    public TEntity Entity { get; private set; }
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }

    public ValChangeNotice(TEntity entity, TProperty newVal, TProperty oldVal)
    {
        Entity = entity;
        NewVal = newVal;
        OldVal = oldVal;
    }
    public void Clear()
    {
        NewVal = default;
        OldVal = default;
    }
}