using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
[MessagePack.Union(0, typeof(ModelMakeProject))]
[MessagePack.Union(1, typeof(PlayerBuildingMakeProject))]
[MessagePack.Union(2, typeof(UnitMakeProject))]

public abstract class MakeProject : IPolymorph
{
    public ERef<Regime> Regime { get; private set; }
    public IdRef Making { get; protected set; }
    public float Amount { get; private set; }
    public float Fulfilled { get; protected set; }

    [SerializationConstructor] protected MakeProject(
        ERef<Regime> regime, 
        IdRef making,
        float amount, float fulfilled)
    {
        Regime = regime;
        Amount = amount;
        Making = making;
        Fulfilled = fulfilled;
    }

    public abstract void Start(ProcedureWriteKey key);
    public abstract void Increment(float amount, 
        ProductionResult result, LogicWriteKey key);

    public abstract void Finish(LogicWriteKey key);
    public abstract Control GetDisplay(Data d);
    
}