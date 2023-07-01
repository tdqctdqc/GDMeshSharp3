using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public EntityRef<MapPolygon> Poly { get; protected set; }
    public ModelRef<SettlementTier> Tier { get; private set; }
    public int Size { get; protected set; }
    public string Name { get; protected set; }
    
    public static Settlement Create(string name, MapPolygon poly, int size, CreateWriteKey key)
    {
        var tier = key.Data.Models.SettlementTiers.GetTier(size);
        var s = new Settlement(key.IdDispenser.GetID(), poly.MakeRef(), 
            size, tier.MakeRef(), name);
        key.Create(s);
        return s;
    }
    [SerializationConstructor] private Settlement(int id, EntityRef<MapPolygon> poly, int size,
        ModelRef<SettlementTier> tier, 
        string name) : base(id)
    {
        Tier = tier;
        Name = name;
        Poly = poly;
        Size = size;
    }

    public void SetName(string name, GenWriteKey key)
    {
        Name = name;
    }
}