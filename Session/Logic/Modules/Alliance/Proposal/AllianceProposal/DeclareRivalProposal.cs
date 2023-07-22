using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeclareRivalProposal : AllianceProposal
{
    public int TargetAllianceId { get; private set; }
    public static DeclareRivalProposal Construct(Regime proposer, Alliance target, Data data)
    {
        var p = new DeclareRivalProposal(-1, target.Id, proposer.MakeRef(), proposer.GetAlliance(data).Id,
            new HashSet<int>(), new HashSet<int>(), 0f);
        return p;
    }
    [SerializationConstructor] private DeclareRivalProposal(int id, int targetAllianceId, EntityRef<Regime> proposer, int allianceId, 
        HashSet<int> inFavor, HashSet<int> against, float priority) 
        : base(id, proposer, allianceId, inFavor, against, priority)
    {
        TargetAllianceId = targetAllianceId;
    }

    public override bool GetDecisionForAi(Regime r, Data d)
    {
        return true;
    }

    protected override void ResolveInner(bool accepted, ProcedureWriteKey key)
    {
        var alliance = key.Data.Society.Alliances[AllianceId];
        var target = key.Data.Society.Alliances[TargetAllianceId];
        if (accepted)
        {
            alliance.Rivals.Add(alliance, target, key);
            target.Rivals.Add(target, alliance, key);
        }
    }

    public override bool Valid(Data data)
    {
        return base.Valid(data) 
               && data.Entities.ContainsKey(TargetAllianceId)
               && data.Society.Alliances[AllianceId].Members.RefIds.Contains(TargetAllianceId) == false;
    }
}
