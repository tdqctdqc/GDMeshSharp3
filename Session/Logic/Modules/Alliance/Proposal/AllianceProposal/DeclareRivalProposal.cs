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
        var alliance = key.Data.Get<Alliance>(AllianceId);
        var target = key.Data.Get<Alliance>(TargetAllianceId);
        if (accepted)
        {
            alliance.Rivals.Add(target, key);
            target.Rivals.Add(alliance, key);
        }
    }

    public override bool Valid(Data data)
    {
        return base.Valid(data) 
               && data.EntitiesById.ContainsKey(TargetAllianceId)
               && data.Get<Alliance>(AllianceId).Members.RefIds.Contains(TargetAllianceId) == false;
    }
}
