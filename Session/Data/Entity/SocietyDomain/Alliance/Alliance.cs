using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Alliance : Entity
{
    public EntityRef<Regime> Leader { get; private set; }
    public EntityRefCollection<Regime> Members { get; private set; }
    
    public static Alliance Create(Regime founder, CreateWriteKey key)
    {
        var col = EntityRefCollection<Regime>.Construct(new HashSet<int>(), key.Data);
        col.AddRef(founder, key);
        var a = new Alliance(founder.MakeRef(), col, key.IdDispenser.GetID());
        key.Create(a);
        return a;
    }
    [SerializationConstructor] private Alliance(EntityRef<Regime> leader,
        EntityRefCollection<Regime> members,  int id) : base(id)
    {
        Leader = leader;
        Members = members;
    }

    public void AddMember(Regime r, ProcedureWriteKey key)
    {
        var old = r.GetAlliance(key.Data);
        if (old != null)
        {
            old.RemoveMember(r, key);
        }
        Members.AddRef(r, key);
    }

    public void RemoveMember(Regime r, ProcedureWriteKey key)
    {
        
    }

    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
}
