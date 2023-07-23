using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class AllianceMergeProposal : DiplomacyProposal
{
    public static AllianceMergeProposal Construct(Regime proposer, Alliance target, Data data)
    {
        var p = new AllianceMergeProposal(proposer.GetAlliance(data).Id, target.Id, -1, proposer.MakeRef(),
            new HashSet<int>(), new HashSet<int>(), 0f);
        return p;
    }
    [SerializationConstructor] private AllianceMergeProposal(int alliance0, int alliance1, int id, EntityRef<Regime> proposer, 
        HashSet<int> inFavor, HashSet<int> against, float priority) 
        : base(alliance0, alliance1, id, proposer, inFavor, 
            against, priority)
    {
    }

    public override bool GetDecisionForAi(Regime r, Data d)
    {
        return true;
    }

    protected override void ResolveInner(bool accepted, ProcedureWriteKey key)
    {
        if (accepted)
        {
            var alliance0 = key.Data.Get<Alliance>(Alliance0);
            var alliance1 = key.Data.Get<Alliance>(Alliance1);
            var members0 = alliance0.Members.Entities(key.Data).ToList();
            
            for (var i = 0; i < members0.Count; i++)
            {
                var r = members0[i];
                alliance0.Members.Remove(r, key);
                alliance1.Members.Add(r, key);
            }
            
            var enemies0 = alliance0.Rivals.Entities(key.Data).ToList();
            for (var i = 0; i < enemies0.Count; i++)
            {
                var e = enemies0[i];
                e.Rivals.Remove(e, key);
                alliance1.Rivals.Add(e, key);
                e.Rivals.Add(alliance1, key);
            }
            
            var war0 = alliance0.AtWar.Entities(key.Data).ToList();
            for (var i = 0; i < war0.Count; i++)
            {
                var e = war0[i];
                e.AtWar.Remove(alliance0, key);
                alliance1.AtWar.Add(e, key);
                e.AtWar.Add(alliance1, key);
            }
            key.Data.RemoveEntity(alliance0.Id, key);
        }
    }

    public override bool Valid(Data data)
    {
        if (data.EntitiesById.ContainsKey(Alliance0) == false) return false;
        if (data.EntitiesById.ContainsKey(Alliance1) == false) return false;
        var a0 = (Alliance) data.EntitiesById[Alliance0];
        var a1 = (Alliance) data.EntitiesById[Alliance1];
        return a0.Rivals.Contains(a1) == false;
    }
}
