using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public class EntityIds : Entity
{
    public int Index => Dispenser.Index;
    public IdDispenser Dispenser { get; private set; }
    public static EntityIds Create(GenWriteKey key)
    {
        var d = new EntityIds(0, new IdDispenser(0));
        key.Create(d);
        d.Id = d.TakeId();
        return d;
    }
    [SerializationConstructor] private EntityIds(int id, IdDispenser dispenser)
        : base(id)
    {
        Dispenser = dispenser;
    }

    public int TakeId()
    {
        return Dispenser.TakeId();
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}