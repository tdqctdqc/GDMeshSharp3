using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
[MessagePack.Union(0, typeof(ModelMakeProject))]
[MessagePack.Union(1, typeof(PlayerBuildingMakeProject))]
[MessagePack.Union(2, typeof(UnitMakeProject))]

public abstract class MakeProject : IPolymorph, IIdentifiable
{
    public ERef<Regime> Regime { get; private set; }
    public IdRef Making { get; protected set; }
    public float Amount { get; private set; }
    public float Fulfilled { get; protected set; }
    public int Id { get; private set; }

    [SerializationConstructor] protected MakeProject(
        ERef<Regime> regime, 
        IdRef making,
        float amount, float fulfilled,
        int id)
    {
        Regime = regime;
        Amount = amount;
        Making = making;
        Fulfilled = fulfilled;
        Id = id;
    }

    public abstract void Start(ProcedureWriteKey key);
    public abstract void Increment(float amount, 
        ProductionResult result, LogicWriteKey key);
    public abstract void Finish(LogicWriteKey key);
    public abstract void Cancel(ProcedureWriteKey key);

    public abstract Control GetDisplay(Data d);

    public void SetId(LogicWriteKey key)
    {
        Id = key.Data.IdDispenser.TakeId();
    }
}