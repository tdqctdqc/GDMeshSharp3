using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class Proposal
{
    public int Id { get; private set; }
    public EntityRef<Regime> Proposer { get; protected set; }
    public HashSet<int> InFavor { get; protected set; }
    public HashSet<int> Against { get; protected set; }
    public float Priority { get; protected set; }

    [SerializationConstructor] protected Proposal(int id, EntityRef<Regime> proposer, 
        HashSet<int> inFavor, HashSet<int> against, float priority)
    {
        Id = id;
        Proposer = proposer;
        InFavor = inFavor;
        Against = against;
        Priority = priority;
    }
    public void UpdatePriority(float newPriority, ProcedureWriteKey key)
    {
        Priority = newPriority;
    }
    public abstract bool GetDecisionForAi(Regime r, Data d);
    public abstract void Propose(ProcedureWriteKey key);

    public void Resolve(bool accepted, ProcedureWriteKey key)
    {
        if (Valid(key.Data))
        {
            ResolveInner(accepted, key);
        }
        CleanUp(key);
    }

    public void Decide(int regime, bool inFavor, ProcedureWriteKey key)
    {
        InFavor.Remove(regime);
        Against.Remove(regime);

        if (inFavor)
        {
            InFavor.Add(regime);
        } 
        else
        {
            Against.Add(regime);
        };
    }
    public void SetId(int id)
    {
        Id = id;
    }

    public abstract bool Undecided(Data data);
    public bool AllianceUndecided(Alliance alliance, Data data)
    {
        var allianceWeight = alliance.Members.Items(data).Sum(m => alliance.GetWeightInAlliance(m, data));
        
        var inFavor = InFavor.Where(f => alliance.Members.RefIds.Contains(f));
        var against = Against.Where(f => alliance.Members.RefIds.Contains(f));
        
        var forWeight = inFavor.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        var forRatio = forWeight / allianceWeight;
        
        var againstWeight = against.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        var againstRatio = againstWeight / allianceWeight;
        
        return forRatio < .5f && againstRatio < .5f;
    }
    public abstract TriBool GetResolution(Data data);
    
    protected abstract void ResolveInner(bool accepted, ProcedureWriteKey key);
    public abstract void CleanUp(ProcedureWriteKey key);
    public abstract float GetPriorityGrowth(Data data);
    public abstract bool Valid(Data data);
}
