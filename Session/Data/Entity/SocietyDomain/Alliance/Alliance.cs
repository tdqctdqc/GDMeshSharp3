using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public EntityRef<Regime> Leader { get; private set; }
    public EntRefCol<Regime> Members { get; private set; }
    public EntRefCol<Alliance> Rivals { get; private set; }
    public EntRefCol<Alliance> AtWar { get; private set; }
    public EntRefCol<Holder<Proposal>> Proposals { get; private set; }
    
    public static Alliance Create(Regime founder, CreateWriteKey key)
    {
        var members = EntRefCol<Regime>.Construct(nameof(Members), -1,
            new HashSet<int>{founder.Id}, key.Data);
        var enemies = EntRefCol<Alliance>.Construct(nameof(Rivals), -1,
            new HashSet<int>{}, key.Data);
        var atWar = EntRefCol<Alliance>.Construct(nameof(AtWar), -1,
            new HashSet<int>{}, key.Data);
        var proposals = EntRefCol<Holder<Proposal>>.Construct(nameof(Proposals), -1,
            new HashSet<int>(), key.Data);
        
        var a = new Alliance(founder.MakeRef(), members, enemies, atWar,
            proposals,
            -1);
        
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(EntityRef<Regime> leader,
        EntRefCol<Regime> members, 
        EntRefCol<Alliance> rivals, 
        EntRefCol<Alliance> atWar, 
        EntRefCol<Holder<Proposal>> proposals,
        int id) : base(id)
    {
        Leader = leader;
        Members = members;
        Proposals = proposals;
        Rivals = rivals;
        AtWar = atWar;
    }

    public void AddMember(Regime r, ProcedureWriteKey key)
    {
        var old = r.GetAlliance(key.Data);
        if (old != null)
        {
            old.RemoveMember(r, key);
        }
        Members.Add(r, key);
    }

    public void RemoveMember(Regime r, ProcedureWriteKey key)
    {
        if (Leader.RefId == r.Id) throw new Exception();
        Members.Remove(r, key);
    }

    public void SetRival(Alliance a, ProcedureWriteKey key)
    {
        if (a == this) throw new Exception();
        Rivals.Add(a, key);
    }
    public void SetWar(Alliance a, ProcedureWriteKey key)
    {
        if (a == this) throw new Exception();
        if(Rivals.Contains(a) == false) throw new Exception();
        AtWar.Add(a, key);
    }
}
