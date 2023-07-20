using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class DeclareEnemyProposal : AllianceProposal
{
    public int TargetAlliance { get; private set; }
    public static DeclareEnemyProposal Construct(Regime proposer, Alliance target, Data data)
    {
        var p = new DeclareEnemyProposal(-1, target.Id, proposer.MakeRef(), proposer.GetAlliance(data).Id,
            new HashSet<int>(), new HashSet<int>(), new HashSet<int>(), 0f);
        return p;
    }
    [SerializationConstructor] private DeclareEnemyProposal(int id, int targetAlliance, EntityRef<Regime> proposer, int alliance, 
        HashSet<int> inFavor, HashSet<int> against, HashSet<int> abstain, float priority) 
        : base(id, proposer, alliance, inFavor, against, abstain, priority)
    {
        TargetAlliance = targetAlliance;
    }

    public override bool GetDecisionForAi(Regime r, Data d)
    {
        return true;
    }

    public override void Resolve(bool accepted, ProcedureWriteKey key)
    {
        if (accepted)
        {
            var alliance = key.Data.Society.Alliances[Alliance];
            var target = key.Data.Society.Alliances[TargetAlliance];
            alliance.Enemies.Add(alliance, target, key);
            target.Enemies.Add(target, alliance, key);
        }
    }
}
