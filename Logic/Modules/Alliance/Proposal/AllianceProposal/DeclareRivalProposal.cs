using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using MessagePack;

public class DeclareRivalProposal : AllianceProposal
{
    public int TargetAllianceId { get; private set; }
    public static DeclareRivalProposal Construct(Regime proposer, Alliance target, Data data)
    {
        var p = new DeclareRivalProposal(-1, target.Id, proposer.MakeRef(), proposer.GetAlliance(data).Id,
            new HashSet<int>(), new HashSet<int>());
        return p;
    }
    [SerializationConstructor] private DeclareRivalProposal(int id, int targetAllianceId, ERef<Regime> proposer, int allianceId, 
        HashSet<int> inFavor, HashSet<int> against) 
        : base(id, proposer, allianceId, inFavor, against)
    {
        TargetAllianceId = targetAllianceId;
    }

    public override bool GetDecisionForAi(Regime r, Data d)
    {
        return true;
    }

    public override Control GetDisplay(Data d)
    {
        var c = new VBoxContainer();
        var sb = new StringBuilder();
        sb.Append($"\nDeclaring rival {TargetAllianceId}");
        sb.Append($"\n In Favor: ");
        foreach (var regime in InFavor.Select(id => d.Get<Regime>(id)))
        {
            sb.Append("\n\t" + regime.Name);
        }
        sb.Append($"\n Against: ");
        foreach (var regime in Against.Select(id => d.Get<Regime>(id)))
        {
            sb.Append("\n\t" + regime.Name);
        }
        NodeExt.CreateLabelAsChild(c, sb.ToString());
        return c;
    }

    protected override void ResolveInner(bool accepted, ProcedureWriteKey key)
    {
        if (accepted)
        {
            var alliance = key.Data.Get<Alliance>(AllianceId);
            var target = key.Data.Get<Alliance>(TargetAllianceId);
            new DeclareRivalProcedure(AllianceId, TargetAllianceId).Enact(key);
            key.Data.Logger.Log($"{alliance.Leader.Entity(key.Data).Name} and {alliance.Leader.Entity(key.Data).Capital.RefId} " +
                                $"{target.Leader.Entity(key.Data).Name} {target.Leader.Entity(key.Data).Capital.RefId} " +
                                $"are now rivals", LogType.Diplomacy);
        }
    }

    public override bool Valid(Data data)
    {
        return base.Valid(data) 
               && data.HasEntity(TargetAllianceId)
               && data.HasEntity(AllianceId)
               && data.Get<Alliance>(AllianceId).Members.RefIds.Contains(TargetAllianceId) == false;
    }
}
