using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public EntityRef<Regime> Leader { get; private set; }
    public EntityRefCollection<Regime> Members { get; private set; }
    public EntityRefCollection<Alliance> Enemies { get; private set; }
    public EntityRefCollection<Alliance> AtWar { get; private set; }
    public Dictionary<int, Proposal> Proposals { get; private set; }
    
    public static Alliance Create(Regime founder, CreateWriteKey key)
    {
        var members = EntityRefCollection<Regime>.Construct(nameof(Members), new HashSet<int>{founder.Id}, key.Data);
        var enemies = EntityRefCollection<Alliance>.Construct(nameof(Enemies), new HashSet<int>{}, key.Data);
        var atWar = EntityRefCollection<Alliance>.Construct(nameof(AtWar), new HashSet<int>{}, key.Data);
        var proposals = new Dictionary<int, Proposal>();
        
        var a = new Alliance(founder.MakeRef(), members, enemies, atWar,
            proposals,
            key.IdDispenser.GetID());
        
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(EntityRef<Regime> leader,
        EntityRefCollection<Regime> members, 
        EntityRefCollection<Alliance> enemies, 
        EntityRefCollection<Alliance> atWar, 
        Dictionary<int, Proposal> proposals,
        int id) : base(id)
    {
        Leader = leader;
        Members = members;
        Proposals = proposals;
        Enemies = enemies;
        AtWar = atWar;
    }

    public void AddMember(Regime r, ProcedureWriteKey key)
    {
        var old = r.GetAlliance(key.Data);
        if (old != null)
        {
            old.RemoveMember(r, key);
        }
        Members.Add(this, r, key);
    }

    public void RemoveMember(Regime r, ProcedureWriteKey key)
    {
        if (Leader.RefId == r.Id) throw new Exception();
        Members.Remove(this, r, key);
    }

    public void SetEnemy(Alliance a, ProcedureWriteKey key)
    {
        if (a == this) throw new Exception();
        Enemies.Add(this, a, key);
    }
    public void SetWar(Alliance a, ProcedureWriteKey key)
    {
        if (a == this) throw new Exception();
        if(Enemies.Contains(a) == false) throw new Exception();
        AtWar.Add(this, a, key);
    }
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
}
