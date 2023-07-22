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
            targetAlliance.Id, new HashSet<int>(), new HashSet<int>(), 0f);
    }
    [SerializationConstructor] private DeclareWarProposal(int id, EntityRef<Regime> proposer, int allianceId, 
        int targetAllianceId, HashSet<int> inFavor, HashSet<int> against, float priority) 
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
        alliance.SetWar(target, key);
        target.SetWar(alliance, key);
    }

    public override bool Valid(Data data)
    {
        return base.Valid(data)
               && data.Entities.ContainsKey(TargetAllianceId)
               && inner();
        bool inner()
        {
            var alliance = data.Society.Alliances[AllianceId];
            var target = data.Society.Alliances[TargetAllianceId];
            return alliance.Rivals.Contains(target);
        }
    }
    
}
