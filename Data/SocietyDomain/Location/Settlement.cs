using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Settlement : Location, INamed
{
    public CellRef Cell { get; protected set; }
    public ModelRef<SettlementTier> Tier { get; private set; }
    public IdCount<SettlementBuilding> Buildings { get; private set; }
    public string Name { get; protected set; }
    
    public static Settlement Create(string name, 
        Cell cell, int size, GenKey key)
    {
        var tier = SettlementTier.GetTier(size, key.Data);
        var s = new Settlement(key.Data.IdDispenser.TakeId(),
            cell.MakeRef(), 
            tier.MakeRef(), 
            IdCount<SettlementBuilding>.Construct(), 
            name);
        key.Create(s);
        return s;
    }
    [SerializationConstructor] private Settlement(int id, 
        CellRef cell,
        ModelRef<SettlementTier> tier, 
        IdCount<SettlementBuilding> buildings, 
        string name) : base(id)
    {
        Buildings = buildings;
        Tier = tier;
        Name = name;
        Cell = cell;
    }

    public void SetName(string name, GenKey key)
    {
        Name = name;
    }

    public void SetTier(SettlementTier tier, ProcedureKey key)
    {
        var old = Tier.Get(key.Data);
        Tier = tier.MakeRef();
        key.Data.Notices.Infrastructure.ChangedTier.Invoke(this, tier, old);
    }

    public override void CleanUp(IWriteKey key)
    {
        
    }
}