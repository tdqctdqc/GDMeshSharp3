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
            var alliance0 = key.Data.Society.Alliances[Alliance0];
            var alliance1 = key.Data.Society.Alliances[Alliance1];
            var members0 = alliance0.Members.Entities(key.Data).ToList();
            
            for (var i = 0; i < members0.Count; i++)
            {
                var r = members0[i];
                alliance0.Members.Remove(alliance0, r, key);
                alliance1.Members.Add(alliance1, r, key);
            }
            
            var enemies0 = alliance0.Enemies.Entities(key.Data).ToList();
            for (var i = 0; i < enemies0.Count; i++)
            {
                var e = enemies0[i];
                alliance1.Enemies.Add(alliance1, e, key);
                e.Enemies.Add(e, alliance1, key);
            }
            
            var war0 = alliance0.AtWar.Entities(key.Data).ToList();
            for (var i = 0; i < war0.Count; i++)
            {
                var e = war0[i];
                alliance1.AtWar.Add(alliance1, e, key);
                e.AtWar.Add(e, alliance1, key);
            }
            
            key.Data.RemoveEntity(alliance0.Id, key);
        }
        
    }

    public override bool Valid(Data data)
    {
        if (data.Entities.ContainsKey(Alliance0) == false) return false;
        if (data.Entities.ContainsKey(Alliance1) == false) return false;
        var a0 = (Alliance) data.Entities[Alliance0];
        var a1 = (Alliance) data.Entities[Alliance1];
        return a0.Enemies.Contains(a1) == false;
    }
}
