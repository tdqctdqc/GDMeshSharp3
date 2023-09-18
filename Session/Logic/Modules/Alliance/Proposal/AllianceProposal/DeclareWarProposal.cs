using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class DeclareWarProposal : AllianceProposal
{
    public int TargetAllianceId { get; private set; }

    public static DeclareWarProposal Construct(Regime proposer, Alliance targetAlliance, Data data)
    {
        return new DeclareWarProposal(-1, proposer.MakeRef(), proposer.GetAlliance(data).Id,
            targetAlliance.Id, new HashSet<int>(), new HashSet<int>());
    }
    [SerializationConstructor] private DeclareWarProposal(int id, EntityRef<Regime> proposer, int allianceId, 
        int targetAllianceId, HashSet<int> inFavor, HashSet<int> against) 
        : base(id, proposer, allianceId, inFavor, against)
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
        alliance.AtWar.Add(target, key);
        target.AtWar.Add(alliance, key);
    }

    public override bool Valid(Data data)
    {
        return base.Valid(data)
               && data.EntitiesById.ContainsKey(TargetAllianceId)
               && inner();
        bool inner()
        {
            var alliance = data.Get<Alliance>(AllianceId);
            var target = data.Get<Alliance>(TargetAllianceId);
            return alliance.Rivals.Contains(target);
        }
    }
}
