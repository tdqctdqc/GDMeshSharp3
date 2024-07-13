
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class UnitTemplate : Entity, IMakeable
{
    public string Name { get; private set; }
    public IdCount<Troop> Troops { get; private set; }
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<MoveType> MoveType { get; private set; }
    public TroopDomain Domain { get; private set; }
    public MakeableAttribute Makeable { get; private set; }

    public static UnitTemplate Create(IHostWriteKey key, 
        string name,
        Dictionary<Troop, float> troopCounts,
        TroopDomain domain,
        MoveType moveType,
        Regime regime)
    {
        var costs = IdCount<Item>.Construct();
        foreach (var kvp in troopCounts)
        {
            var troop = kvp.Key;
            var numTroop = kvp.Value;
            costs.Add(troop, numTroop);
        }
        
        var makeable = new MakeableAttribute(
            costs, 
            IdCount<Item>.Construct()
        );
        var u = new UnitTemplate(name, IdCount<Troop>.Construct(troopCounts),
            moveType.MakeRef(), regime.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            domain,
            makeable);
        key.Create(u);
        return u;
    }
    [SerializationConstructor] private UnitTemplate(string name,
        IdCount<Troop> troops, ModelRef<MoveType> moveType,
        ERef<Regime> regime, int id, 
        TroopDomain domain,
        MakeableAttribute makeable) 
        : base(id)
    {
        MoveType = moveType;
        Name = name;
        Troops = troops;
        Regime = regime;
        Domain = domain;
        Makeable = makeable;
    }

    public static void CreateDefaultTemplatesForRegime(Regime r, 
        IHostWriteKey key)
    {
        var inf = Create(key, "Infantry Division",
            new Dictionary<Troop, float>
                {
                    {key.Data.Models.Troops.Rifle1, 100f},
                    {key.Data.Models.Troops.Artillery1, 10f}
                }, key.Data.Models.TroopDomains.Land, key.Data.Models.MoveTypes.InfantryMove,
            r);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }

    public float GetPowerPoints(Data d)
    {
        return Troops.GetEnumModel(d)
            .Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
    }

    public Control GetDisplay(Data d)
    {
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var small = Game.I.Client.Settings.SmallIconSize.Value;
        var vbox = new VBoxContainer();
        var icon = this.GetMaxPowerTroop(d).Icon.GetLabeledIcon<HBoxContainer>(
            $"{Name}",
            large);
        vbox.AddChild(icon);
        vbox.CreateLabelAsChild(Name);
        
        foreach (var (key, value) in Troops.GetEnumModel(d))
        {
            vbox.AddChild(key.Icon.GetLabeledIcon<HBoxContainer>
                ($"{key.Name}: {value.ToString()}", small));
        }
        
        return vbox;
    }
}