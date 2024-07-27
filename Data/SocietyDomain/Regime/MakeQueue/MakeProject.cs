using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
[MessagePack.Union(0, typeof(ModelMakeProject))]
[MessagePack.Union(1, typeof(PlayerSettlementBuildingMakeProject))]
[MessagePack.Union(2, typeof(UnitMakeProject))]
[MessagePack.Union(3, typeof(PlayerResourceExtractionMakeProject))]

public abstract class MakeProject : IPolymorph, IIdentifiable
{
    public ERef<Regime> Regime { get; protected set; }
    // public IdRef Making { get; protected set; }
    public float Amount { get; protected set; }
    public float Fulfilled { get; protected set; }
    public int Id { get; private set; }

    [SerializationConstructor] protected MakeProject(
        ERef<Regime> regime, 
        float amount, float fulfilled,
        int id)
    {
        Regime = regime;
        Amount = amount;
        Fulfilled = fulfilled;
        Id = id;
    }
    
    public abstract void Start(LogicWriteKey key);
    public abstract void Increment(RegimeStock stock, LogicWriteKey key);
    public abstract void Finish(LogicWriteKey key);
    public abstract void Cancel(ProcedureWriteKey key);
    public abstract Control GetDisplay(Data d);
    public abstract bool Consolidate(MakeProject next, LogicWriteKey key);
    public abstract MakeableAttribute GetMakeable(Data d);
    public abstract Icon GetIcon(Data d);
    public abstract string Description(Data d);
    public void SetId(LogicWriteKey key)
    {
        Id = key.Data.IdDispenser.TakeId();
    }
    
}