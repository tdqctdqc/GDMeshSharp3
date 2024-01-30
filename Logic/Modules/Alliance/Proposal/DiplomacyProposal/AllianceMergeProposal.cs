using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using MessagePack;

public class AllianceMergeProposal : DiplomacyProposal
{
    public static AllianceMergeProposal Construct(Regime proposer, Alliance target, Data data)
    {
        var p = new AllianceMergeProposal(proposer.GetAlliance(data).Id, target.Id, 
            -1, proposer.MakeRef(), new HashSet<int>(), new HashSet<int>());
        return p;
    }
    [SerializationConstructor] private AllianceMergeProposal(int alliance0, int alliance1, int id, ERef<Regime> proposer, 
        HashSet<int> inFavor, HashSet<int> against) 
        : base(alliance0, alliance1, id, proposer, inFavor, against)
    {
    }

    public override bool GetDecisionForAi(Regime r, Data d)
    {
        return true;
    }

    public override Control GetDisplay(Data d)
    {
        var c = new VBoxContainer();
        var a0 = d.Get<Alliance>(Alliance0);
        var a1 = d.Get<Alliance>(Alliance1);
        var sb = new StringBuilder();
        sb.Append($"Merge Alliances {a0.Id} and {a1.Id}");
        sb.Append($"In Favor: ");
        foreach (var regime in InFavor.Select(id => d.Get<Regime>(id)))
        {
            sb.Append(regime.Name);
        }
        sb.Append($"Against: ");
        foreach (var regime in Against.Select(id => d.Get<Regime>(id)))
        {
            sb.Append(regime.Name);
        }
        NodeExt.CreateLabelAsChild(c, sb.ToString());
        return c;
    }

    protected override void ResolveInner(bool accepted, ProcedureWriteKey key)
    {
        if (accepted)
        {
            var alliance0 = key.Data.Get<Alliance>(Alliance0);
            var alliance1 = key.Data.Get<Alliance>(Alliance1);
            var members0 = alliance0.Members.Items(key.Data).ToList();
            
            for (var i = 0; i < members0.Count; i++)
            {
                var r = members0[i];
                alliance0.Members.Remove(r, key);
                alliance1.Members.Add(r, key);
            }
            
            key.Data.Society.DiploGraph.MergeRelations(alliance0, alliance1, key);
            key.Data.RemoveEntity(alliance0.Id, key);
        }
    }

    public override bool Valid(Data data)
    {
        if (data.HasEntity(Alliance0) == false) return false;
        if (data.HasEntity(Alliance1) == false) return false;
        var a0 = (Alliance) data.EntitiesById[Alliance0];
        var a1 = (Alliance) data.EntitiesById[Alliance1];
        return a0.IsRivals(a1, data) == false;
    }
}
