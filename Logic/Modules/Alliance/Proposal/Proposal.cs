using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(AllianceMergeProposal))]
[MessagePack.Union(1, typeof(DeclareRivalProposal))]
[MessagePack.Union(2, typeof(DeclareWarProposal))]
public abstract class Proposal : IPolymorph
{
    public int Id { get; private set; }
    public EntityRef<Regime> Proposer { get; protected set; }
    public HashSet<int> AllianceIds { get; private set; }
    public HashSet<int> InFavor { get; protected set; }
    public HashSet<int> Against { get; protected set; }
    [SerializationConstructor] protected Proposal(int id, EntityRef<Regime> proposer, 
        HashSet<int> allianceIds, HashSet<int> inFavor, HashSet<int> against)
    {
        Id = id;
        AllianceIds = allianceIds;
        Proposer = proposer;
        InFavor = inFavor;
        Against = against;
    }
    public abstract bool GetDecisionForAi(Regime r, Data d);
    public abstract void Propose(ProcedureWriteKey key);
    public abstract Control GetDisplay(Data d);
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
    public TriBool AllianceInFavor(Alliance alliance, Data data)
    {
        if (AllianceUndecided(alliance, data))
        {
            return TriBool.Undecided;
        }
        var inFavor = InFavor.Where(f => alliance.Members.RefIds.Contains(f));
        var against = Against.Where(f => alliance.Members.RefIds.Contains(f));
        var undecided = alliance.Members.RefIds.Except(inFavor).Except(against);
        var forWeight = inFavor.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        var againstWeight = against.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        var undecidedWeight = undecided.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        
        return new TriBool(forWeight > againstWeight);
    }
    public void SetId(int id)
    {
        Id = id;
    }

    public bool Undecided(Data data)
    {
        foreach (var allianceId in AllianceIds)
        {
            if (data.EntitiesById.ContainsKey(allianceId))
            {
                if (AllianceUndecided(data.Get<Alliance>(allianceId), data))
                {
                    return true;
                }
            }
        }

        return false;
    }
    
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

    public void CleanUp(ProcedureWriteKey key)
    {
        foreach (var allianceId in AllianceIds)
        {
            if (key.Data.EntitiesById.ContainsKey(allianceId))
            {
                var alliance = key.Data.Get<Alliance>(allianceId);
                alliance.ProposalIds.Remove(Id);
            }
        }
        key.Data.Society.Proposals.Proposals.Remove(Id);
    }
    public abstract float GetPriorityGrowth(Data data);
    public abstract bool Valid(Data data);
}
