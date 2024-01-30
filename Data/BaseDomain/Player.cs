using Godot;
using System;
using MessagePack;

public class Player : Entity
{
    public Guid PlayerGuid { get; private set; }
    public string Name { get; private set; }
    public ERef<Regime> Regime { get; private set; }
    public static Player Create(Guid guid, string name, ICreateWriteKey key)
    {
        var p = new Player(key.Data.IdDispenser.TakeId(), guid, name, new ERef<Regime>(-1));
        key.Create(p);
        return p;
    }

    [SerializationConstructor] private Player(int id, Guid playerGuid, 
        string name, ERef<Regime> regime) : base(id)
    {
        Regime = regime;
        PlayerGuid = playerGuid;
        Name = name;
    }

    public void SetRegime(Regime regime, ProcedureWriteKey key)
    {
        var old = Regime.Entity(key.Data);
        Regime = regime.MakeRef();
        key.Data.BaseDomain.PlayerAux.PlayerChangedRegime.Invoke(this, regime, old);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
