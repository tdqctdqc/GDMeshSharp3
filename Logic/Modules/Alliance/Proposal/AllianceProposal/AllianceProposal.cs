using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public abstract class AllianceProposal : Proposal
{
    public int AllianceId { get; protected set; }   
    [SerializationConstructor] protected AllianceProposal(int id, ERef<Regime> proposer, 
        int allianceId, HashSet<int> inFavor, HashSet<int> against) 
        : base(id, proposer, new HashSet<int>{allianceId}, inFavor, 
            against)
    {
        AllianceId = allianceId;
    }

    public override void Propose(ProcedureWriteKey key)
    {
        var alliance = key.Data.Get<Alliance>(AllianceId);
        var proposer = this.Proposer.RefId;
        InFavor.Add(proposer);
        alliance.ProposalIds.Add(Id);
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
}