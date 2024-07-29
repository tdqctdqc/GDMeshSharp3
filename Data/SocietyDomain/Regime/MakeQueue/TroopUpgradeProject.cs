
using System;
using Godot;

public class TroopUpgradeProject : MakeProject, IMakeable
{
    public MakeableAttribute Makeable { get; private set; }
    public ModelRef<Troop> From { get; private set; }
    public ModelRef<Troop> To { get; private set; }
    public ERef<Unit> Target { get; private set; }
    public static TroopUpgradeProject Construct(Regime regime,
        Troop from, Troop to, float amount,
        Data d, 
        Unit target = null)
    {
        var buildCosts = IdCount<Item>.Construct(to.Makeable.BuildCosts);
        
        foreach (var (key, value) in from.Makeable.BuildCosts.GetEnumModel(d))
        {
            buildCosts.Remove(key, Mathf.Min(value, buildCosts.Get(key)));
        }
        var makeable = new MakeableAttribute(buildCosts, IdCount<Item>.Construct());

        ERef<Unit> targetRef;
        if (target is Unit u)
        {
            targetRef = u.MakeRef();
        }
        else if (target is null)
        {
            targetRef = new ERef<Unit>();
        }
        else
        {
            throw new Exception();
        }
        
        return new TroopUpgradeProject(
            makeable, from.MakeRef(), to.MakeRef(), regime.MakeRef(),
            targetRef, amount, 0, -1);
    }
    
    public TroopUpgradeProject(
        MakeableAttribute makeable,
        ModelRef<Troop> from,
        ModelRef<Troop> to,
        ERef<Regime> regime, 
        ERef<Unit> target,
        float amount, float fulfilled, int id) 
            : base(regime, amount, fulfilled, id)
    {
        Makeable = makeable;
        Target = target;
        From = from;
        To = to;
    }

    public override void Start(LogicKey key)
    {
        
    }

    public override void Increment(RegimeStock stock, LogicKey key)
    {
        var regime = Regime.Get(key.Data);
        var allTroops = regime.GetAllTroopAmounts(key.Data);
        var cap = allTroops.TryGetValue(From.Get(key.Data), out var amt)
            ? amt
            : 0f;
        if (cap == 0f) return;
        var make = Mathf.Min(cap, Amount - Fulfilled);
        var increment = BuildTree.Increment(Makeable,
            regime.Stock, make, key);
        Fulfilled += increment;
        var proc = new UpgradeTroopProcedure(
            Regime, From, To, increment, Target);
        key.SendMessage(proc);
    }

    public override void Finish(LogicKey key)
    {
    }

    public override void Cancel(ProcedureKey key)
    {
    }

    public override Control GetDisplay(Data d)
    {
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var small = Game.I.Client.Settings.SmallIconSize.Value;
        var hbox = new HBoxContainer();
        var from = From.Get(d);
        var to = To.Get(d);
        hbox.AddChild(from.Icon.GetLabeledIcon<HBoxContainer>(from.Name, large));
        hbox.CreateLabelAsChild(" to ");
        hbox.AddChild(to.Icon.GetLabeledIcon<HBoxContainer>(to.Name, large));
        hbox.CreateLabelAsChild($" {Fulfilled} / {Amount}");

        var costs = Makeable.BuildCosts.GetEnumModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = Makeable.BuildCosts.Get(key) * Amount;
            var have = Makeable.BuildCosts.Get(key) * Fulfilled;
            var str = $"{key.Name}: {have} / {needed}";
            if (key is IIconed i)
            {
                hbox.AddChild(i.Icon.GetLabeledIcon<HBoxContainer>(
                    str, small));
            }
            else
            {
                hbox.CreateLabelAsChild(str);
            }
        }
        
        return hbox;
    }

    public override bool Consolidate(MakeProject next, LogicKey key)
    {
        return false;
    }

    public override MakeableAttribute GetMakeable(Data d)
    {
        return Makeable;
    }

    public override Icon GetIcon(Data d)
    {
        return To.Get(d).Icon;
    }

    public override string Description(Data d)
    {
        return $"Upgrade from {From.Get(d).Name} to {To.Get(d).Name}";
    }

}