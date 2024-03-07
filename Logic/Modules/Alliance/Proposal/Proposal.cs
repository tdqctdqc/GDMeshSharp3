using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(AllianceMergeProposal))]
public abstract class Proposal : IPolymorph
{
    public int Id { get; private set; }
    public ERef<Alliance> Proposer { get; protected set; }
    public ERef<Alliance> Target { get; protected set; }
    [SerializationConstructor] protected Proposal(int id, 
        ERef<Alliance> proposer, ERef<Alliance> target)
    {
        Id = id;
        Proposer = proposer;
        Target = target;
    }
    public abstract bool GetDecisionForAi(Data d);
    public abstract Control GetDisplay(Data d);
    public void Resolve(bool accepted, ProcedureWriteKey key)
    {
        if (Valid(key.Data))
        {
            ResolveInner(accepted, key);
        }
        CleanUp(key);
    }
    public void SetId(int id)
    {
        Id = id;
    }
    
    protected abstract void ResolveInner(bool accepted, ProcedureWriteKey key);

    public void CleanUp(ProcedureWriteKey key)
    {
        key.Data.Society.Proposals.Proposals.Remove(Id);
    }
    public abstract bool Valid(Data data);
}
