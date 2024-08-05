
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class UnitTemplate : Entity, INamed
{
    public string Name { get; private set; }
    public IdCount<TroopType> Troops { get; private set; }
    public ERef<Regime> Regime { get; private set; }
    public TroopDomain Domain { get; private set; }
    public static UnitTemplate Create(LogicKey key, 
        string name,
        IdCount<TroopType> troopCounts,
        TroopDomain domain,
        Regime regime)
    {
        var u = new UnitTemplate(name, 
            IdCount<TroopType>.Construct(troopCounts),
            regime.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            domain);
        key.Create(u);
        return u;
    }
    public static UnitTemplate Create(LogicKey key, 
        string name,
        Dictionary<TroopType, float> troopCounts,
        TroopDomain domain,
        Regime regime)
    {
        var u = new UnitTemplate(name, 
            IdCount<TroopType>.Construct(troopCounts),
            regime.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            domain);
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private UnitTemplate(string name,
        IdCount<TroopType> troops,
        ERef<Regime> regime, int id, 
        TroopDomain domain) 
        : base(id)
    {
        Name = name;
        Troops = troops;
        Regime = regime;
        Domain = domain;
    }

    public override void CleanUp(ProcedureKey key)
    {
        
    }
    public Control GetDisplay(Data d)
    {
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var small = Game.I.Client.Settings.SmallIconSize.Value;
        var vbox = new VBoxContainer();
        var icon = GetIcon(d).GetLabeledIcon<HBoxContainer>(
                $"{Name}",
                large);;
        vbox.AddChild(icon);
        vbox.CreateLabelAsChild(Name);
        
        foreach (var (key, value) in Troops.GetEnumModel(d))
        {
            vbox.AddChild(key.Icon.GetLabeledIcon<HBoxContainer>
                ($"{key.Name}: {value.ToString()}", small));
        }
        
        return vbox;
    }

    public Icon GetIcon(Data d)
    {
        if (Troops.Contents.Count == 0) return Icon.Blank;
        return Troops.GetEnumModel(d)
            .MaxBy(v => v.Key.FrontLength * v.Value)
            .Key.Icon;
    }

    public void Rename(string newName, ProcedureKey key)
    {
        Name = newName;
    }
}