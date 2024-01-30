using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using MessagePack;

public class DeclareWarProposal : AllianceProposal
{
    public int TargetAllianceId { get; private set; }

    public static DeclareWarProposal Construct(Regime proposer, Alliance targetAlliance, Data data)
    {
        return new DeclareWarProposal(-1, proposer.MakeRef(), proposer.GetAlliance(data).Id,
            targetAlliance.Id, new HashSet<int>(), new HashSet<int>());
    }
    [SerializationConstructor] private DeclareWarProposal(int id, ERef<Regime> proposer, int allianceId, 
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
        if (accepted)
        {
            var alliance = key.Data.Get<Alliance>(AllianceId);
            var target = key.Data.Get<Alliance>(TargetAllianceId);
            key.Data.Society.DiploGraph.AddEdge(alliance, target, DiploRelation.War, key);            
        }
    }
    public override Control GetDisplay(Data d)
    {
        var c = new VBoxContainer();
        var sb = new StringBuilder();
        sb.Append($"Declaring war on {TargetAllianceId}");
        sb.Append($"\nIn Favor: ");
        foreach (var regime in InFavor.Select(id => d.Get<Regime>(id)))
        {
            sb.Append("\n\t" + regime.Name);
        }
        sb.Append($"\nAgainst: ");
        foreach (var regime in Against.Select(id => d.Get<Regime>(id)))
        {
            sb.Append("\n\t" + regime.Name);
        }
        NodeExt.CreateLabelAsChild(c, sb.ToString());
        return c;
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
            return alliance.IsRivals(target, data);
        }
    }
}
