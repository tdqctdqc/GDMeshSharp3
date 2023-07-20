using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RegimeRelation : Entity
{
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public EntityRef<Regime> HighId { get; protected set; }
    public EntityRef<Regime> LowId { get; protected set; }

    [SerializationConstructor] private RegimeRelation(int id, EntityRef<Regime> lowId, EntityRef<Regime> highId,
        bool atWar, bool openBorders, bool alliance, bool enemies) : base(id)
    {
        if (lowId.RefId == highId.RefId) throw new Exception();
        HighId = lowId.RefId > highId.RefId ? lowId : highId;
        LowId = lowId.RefId > highId.RefId ? highId : lowId;
    }

    public static RegimeRelation Create(int id, EntityRef<Regime> r1, EntityRef<Regime> r2, CreateWriteKey key)
    {
        var rr = new RegimeRelation(id, r1, r2, false, false, false, false);
        key.Create(rr);
        return rr;
    }
}