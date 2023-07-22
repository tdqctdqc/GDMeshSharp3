using Godot;
using System;
using MessagePack;

public class Player : Entity
{
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public Guid PlayerGuid { get; private set; }
    public string Name { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
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

    public void SetRegime(Regime regime, ProcedureWriteKey key)
    {
        var old = Regime.Entity(key.Data);
        Regime = regime.MakeRef();
        key.Data.BaseDomain.PlayerAux.PlayerChangedRegime
            .Invoke(new ValChangeNotice<Regime>(this, regime, old));
    }
    
}
