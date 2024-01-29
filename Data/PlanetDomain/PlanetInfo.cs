using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PlanetInfo : Entity
{
    public Vector2 Dimensions { get; protected set; }
    public static PlanetInfo Create(Vector2 dimensions, ICreateWriteKey key)
    {
        var pi =  new PlanetInfo(key.Data.IdDispenser.TakeId(), dimensions);
        key.Create(pi);
        return pi;
    }
    
    [SerializationConstructor] private PlanetInfo(int id, Vector2 dimensions) : base(id)
    {
        Dimensions = dimensions;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}