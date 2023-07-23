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
        var alliance = key.Data.Society.Alliances[AllianceId];
        var holder = key.Data.GetRegister<Holder<Proposal>>()[Id];
        alliance.Proposals.Add(alliance, holder, key);
    }

    public override void CleanUp(ProcedureWriteKey key)
    {
        var holder = key.Data.GetRegister<Holder<Proposal>>()[Id];

        if (key.Data.Entities.ContainsKey(AllianceId))
        {
            var alliance = key.Data.Society.Alliances[AllianceId];
            alliance.Proposals.Remove(alliance, holder, key);
        }
    }

    public override TriBool GetResolution(Data data)
    {
        var alliance = data.Society.Alliances[AllianceId];
        var forWeight = InFavor.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
        var againstWeight = Against.Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));
        var undecidedWeight = alliance.Members.RefIds.Except(InFavor).Except(Against)
            .Sum(f => alliance.GetWeightInAlliance(data.Society.Regimes[f], data));;
        if (undecidedWeight > forWeight && undecidedWeight > againstWeight)
        {
            return TriBool.Undecided;
        }
        return new TriBool(forWeight > againstWeight);
    }
    public override float GetPriorityGrowth(Data data)
    {
        var alliance = data.Society.Alliances[AllianceId];
        var res = alliance.GetWeightInAlliance(Proposer.Entity(data), data);
        if (Proposer.RefId == alliance.Leader.RefId) res *= 2;
        return res;
    }

    public override bool Valid(Data data)
    {
        return data.Entities.ContainsKey(AllianceId);
    }

    public override bool Undecided(Data data)
    {
        return AllianceUndecided(data.Society.Alliances[AllianceId], data);
    }
}