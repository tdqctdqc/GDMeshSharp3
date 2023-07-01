using Godot;
using System;
using MessagePack;

public class Player : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(BaseDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public Guid PlayerGuid { get; protected set; }
    public string Name { get; protected set; }
    public EntityRef<Regime> Regime { get; protected set; }
    public static Player Create(Guid guid, string name, 
        CreateWriteKey key)
    {
        var p = new Player(key.IdDispenser.GetID(), guid, name, new EntityRef<Regime>(-1));
        key.Create(p);
        return p;
    }

    [SerializationConstructor] private Player(int id, Guid playerGuid, 
        string name, EntityRef<Regime> regime) : base(id)
    {
        Regime = regime;
        PlayerGuid = playerGuid;
        Name = name;
    }
    
}
