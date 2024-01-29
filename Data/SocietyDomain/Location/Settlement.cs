using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location
{
    public EntityRef<MapPolygon> Poly { get; protected set; }
    public ModelRef<SettlementTier> Tier { get; private set; }
    public string Name { get; protected set; }
    
    public static Settlement Create(string name, 
        MapPolygon poly, int size, ICreateWriteKey key)
    {
        var tier = key.Data.Models.Settlements.GetTier(size);
        var s = new Settlement(key.Data.IdDispenser.TakeId(), poly.MakeRef(), 
            tier.MakeRef(), name);
        key.Create(s);
        return s;
    }
    [SerializationConstructor] private Settlement(int id, EntityRef<MapPolygon> poly,
        ModelRef<SettlementTier> tier, 
        string name) : base(id)
    {
        Tier = tier;
        Name = name;
        Poly = poly;
    }

    public void SetName(string name, GenWriteKey key)
    {
        Name = name;
    }

    public void SetTier(SettlementTier tier, ProcedureWriteKey key)
    {
        var old = Tier.Model(key.Data);
        Tier = tier.MakeRef();
        key.Data.Infrastructure.SettlementAux.ChangedTier.Invoke(this, tier, old);
    }

    public void SetSizeGen(int size, GenWriteKey key)
    {
        var tier = key.Data.Models.Settlements.GetTier(size);
        Tier = tier.MakeRef();
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}