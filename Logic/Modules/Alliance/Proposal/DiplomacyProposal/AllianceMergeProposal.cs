using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using MessagePack;

public class AllianceMergeProposal : Proposal
{
    public static AllianceMergeProposal Construct(Alliance proposer, Alliance target, Data data)
    {
        var p = new AllianceMergeProposal(
            -1, 
            proposer.MakeRef(),
            target.MakeRef());
        return p;
    }
    [SerializationConstructor] private AllianceMergeProposal(int id, 
        ERef<Alliance> proposer, 
        ERef<Alliance> target) 
        : base(id, proposer, target)
    {
    }

    public override bool GetDecisionForAi(Data d)
    {
        return true;
    }

    public override Control GetDisplay(Data d)
    {
        var c = new VBoxContainer();
        NodeExt.CreateLabelAsChild(c, $"Merge Alliance {Target.RefId} into {Proposer.RefId}");
        return c;
    }

    protected override void ResolveInner(bool accepted, ProcedureWriteKey key)
    {
        if (accepted)
        {
            var target = Target.Entity(key.Data);
            var proposer = Proposer.Entity(key.Data);
            var targetMembers = target.Members.Items(key.Data).ToList();
            
            for (var i = 0; i < targetMembers.Count; i++)
            {
                var r = targetMembers[i];
                target.Members.Remove(r, key);
                proposer.Members.Add(r, key);
            }
            
            key.Data.Society.DiploGraph.MergeRelations(target, proposer, key);
            key.Data.RemoveEntity(target.Id, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(Target.RefId) == false)
        {
            error = "Target alliance not found";
            return false;
        }
        if (data.HasEntity(Proposer.RefId) == false)
        {
            error = "Proposer alliance not found";
            return false;
        }
        var target = (Alliance) data.EntitiesById[Target.RefId];
        var targetLeader = target.Leader.Entity(data);
        if (targetLeader.IsMajor)
        {
            error = "Target leader is major regime";
            return false;
        }
        var proposer = (Alliance) data.EntitiesById[Proposer.RefId];
        var proposerLeader = proposer.Leader.Entity(data);
        if (proposerLeader.IsMajor == false)
        {
            error = "Proposer leader is not major regime";
        }
        
        if(target.IsRivals(proposer, data))
        {
            error = "Target and proposer are rivals";
            return false;
        }

        error = "";
        return true;
    }
}
