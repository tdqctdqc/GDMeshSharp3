using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PlanetInfo : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public Vector2 Dimensions { get; protected set; }
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public static PlanetInfo Create(Vector2 dimensions, CreateWriteKey key)
    {
        var pi =  new PlanetInfo(key.IdDispenser.GetID(), dimensions);
        key.Create(pi);
        return pi;
    }
    
    [SerializationConstructor] private PlanetInfo(int id, Vector2 dimensions) : base(id)
    {
        Dimensions = dimensions;
    }
}