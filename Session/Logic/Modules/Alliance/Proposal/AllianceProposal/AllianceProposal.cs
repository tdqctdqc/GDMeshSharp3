using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public abstract class AllianceProposal : Proposal
{
    public int AllianceId { get; protected set; }   
    [SerializationConstructor] protected AllianceProposal(int id, EntityRef<Regime> proposer, 
        int allianceId, HashSet<int> inFavor, HashSet<int> against, float priority) 
        : base(id, proposer, inFavor, against, priority)
    {
        AllianceId = allianceId;
    }

    public override void Propose(ProcedureWriteKey key)
    {
        var alliance = key.Data.Get<Alliance>(AllianceId);
        var holder = key.Data.Get<Holder<Proposal>>(Id);
        alliance.Proposals.Add(holder, key);
    }

    public override void CleanUp(ProcedureWriteKey key)
    {
        var holder = key.Data.Get<Holder<Proposal>>(Id);

        if (key.Data.EntitiesById.ContainsKey(AllianceId))
        {
            var alliance = key.Data.Get<Alliance>(AllianceId);
            alliance.Proposals.Remove(holder, key);
        }
    }

    public override TriBool GetResolution(Data data)
    {
        var alliance = data.Get<Alliance>(AllianceId);
        var forWeight = InFavor.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        var againstWeight = Against.Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));
        var undecidedWeight = alliance.Members.RefIds.Except(InFavor).Except(Against)
            .Sum(f => alliance.GetWeightInAlliance(data.Get<Regime>(f), data));;
        if (undecidedWeight > forWeight && undecidedWeight > againstWeight)
        {
            return TriBool.Undecided;
        }
        return new TriBool(forWeight > againstWeight);
    }
    public override float GetPriorityGrowth(Data data)
    {
        var alliance = data.Get<Alliance>(AllianceId);
        var res = alliance.GetWeightInAlliance(Proposer.Entity(data), data);
        if (Proposer.RefId == alliance.Leader.RefId) res *= 2;
        return res;
    }

    public override bool Valid(Data data)
    {
        return data.EntitiesById.ContainsKey(AllianceId);
    }

    public override bool Undecided(Data data)
    {
        return AllianceUndecided(data.Get<Alliance>(AllianceId), data);
    }
}