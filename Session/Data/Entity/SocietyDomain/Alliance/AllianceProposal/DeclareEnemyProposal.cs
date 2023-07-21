using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeclareEnemyProposal : AllianceProposal
{
    public int TargetAllianceId { get; private set; }
    public static DeclareEnemyProposal Construct(Regime proposer, Alliance target, Data data)
    {
        var p = new DeclareEnemyProposal(-1, target.Id, proposer.MakeRef(), proposer.GetAlliance(data).Id,
            new HashSet<int>(), new HashSet<int>(), 0f);
        return p;
    }
    [SerializationConstructor] private DeclareEnemyProposal(int id, int targetAllianceId, EntityRef<Regime> proposer, int allianceId, 
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
            alliance.Enemies.Add(alliance, target, key);
            target.Enemies.Add(target, alliance, key);
        }
    }

    public override bool Valid(Data data)
    {
        if (data.Entities.ContainsKey(AllianceId) == false) return false;
        if (data.Entities.ContainsKey(TargetAllianceId) == false) return false;
        var a0 = (Alliance) data.Entities[AllianceId];
        var a1 = (Alliance) data.Entities[TargetAllianceId];
        return data.Entities.ContainsKey(AllianceId) &&
               data.Entities.ContainsKey(TargetAllianceId);
    }
}
