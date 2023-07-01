using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public abstract class ValChangeNotice : IEntityNotice
{
    public string FieldName { get; protected set; }
    Type IEntityNotice.EntityType => Entity.GetType();
    public Entity Entity { get; private set; }
    protected ValChangeNotice(string fieldName, Entity entity)
    {
        FieldName = fieldName;
        Entity = entity;
    }

    public abstract void Clear();
}

public class ValChangeNotice<TProperty> : ValChangeNotice
{
    public TProperty NewVal { get; private set; }
    public TProperty OldVal { get; private set; }

    public ValChangeNotice(Entity entity, string fieldName, TProperty newVal, TProperty oldVal) 
        : base(fieldName, entity)
    {
        NewVal = newVal;
        OldVal = oldVal;
    }
    public override void Clear()
    {
        NewVal = default;
        OldVal = default;
        FieldName = "";
    }
}