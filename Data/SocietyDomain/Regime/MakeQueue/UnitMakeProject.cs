
using System.Linq;
using Godot;
using MessagePack;

public class UnitMakeProject : MakeProject, IMakeable
{
    public IdCount<Troop> Troops { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public MakeableAttribute MakeableBase { get; private set; }
    public ERef<UnitTemplate> Template { get; private set; }
    public static UnitMakeProject Construct(Regime r,
        UnitTemplate template,
        int amount, Data d)
    {
        var troops = IdCount<Troop>.Construct();
        foreach (var (troopType, value) in template.Troops.GetEnumModel(d))
        {
            var troop = r.Military.GetBestTroopOfType(troopType, d);
            troops.Add(troop, value);
        }

        var baseCosts = IdCount<Item>.Construct();
        
        foreach (var (troop, amtTroop) in troops.GetEnumModel(d))
        {
            foreach (var (item, amtItem) in troop.Makeable.BuildCosts.GetEnumModel(d))
            {
                baseCosts.Add(item, amtTroop * amtItem);
            }
        }
        
        var makeable = new MakeableAttribute(
            IdCount<Item>.Construct(troops),
            IdCount<Item>.Construct()
        );

        var makeableBase = new MakeableAttribute(
            baseCosts,
            IdCount<Item>.Construct()
        );
        
        return new UnitMakeProject(r.MakeRef(), 
            template.MakeRef(), troops,
            makeable,
            makeableBase,
            amount, 0f, -1);
    }
    [SerializationConstructor] private UnitMakeProject(
        ERef<Regime> regime, 
        ERef<UnitTemplate> template, IdCount<Troop> troops, 
        MakeableAttribute makeable,
        MakeableAttribute makeableBase,
        float amount, float fulfilled, int id) 
            : base(regime, amount, fulfilled, id)
    {
        Template = template;
        Makeable = makeable;
        MakeableBase = makeableBase;
        Troops = troops;
    }

    public override void Start(LogicWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        Increment(regime.Stock, key);
        var setStock = new SetStockProcedure(regime.MakeRef(),
            regime.Stock);
        key.SendMessage(setStock);
    }

    public override void Increment(RegimeStock stock,
        LogicWriteKey key)
    {
        var before = Mathf.FloorToInt(Fulfilled);
        Fulfilled += BuildTree.Increment(Makeable,
            stock,
            Amount - Fulfilled,
            key);
        var after = Mathf.FloorToInt(Fulfilled);
        var made = after - before;
        for (var i = 0; i < made; i++)
        {
            Unit.Create(Template.Get(key.Data),
                Troops, Regime.Get(key.Data), key);
        }
    }

    public override void Finish(LogicWriteKey key)
    {
        
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        var making = Template.Get(key.Data);
        var stock = Regime.Get(key.Data).Stock;
        var numMade = Mathf.FloorToInt(Fulfilled);
        var diff = Fulfilled - numMade;
        foreach (var (model, amt) in Troops.GetEnumModel(key.Data))
        {
            var spent = numMade * amt;
            stock.Stock.Add(model, spent);
        }
    }

    public override Control GetDisplay(Data d)
    {
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var small = Game.I.Client.Settings.SmallIconSize.Value;
        var m = Template.Get(d);
        var vbox = new VBoxContainer();
        var icon = m.GetIcon(d).GetLabeledIcon<HBoxContainer>(
                $"{m.Name}: {Fulfilled} / {Amount} ",
                large);
        vbox.AddChild(icon);
        vbox.CreateLabelAsChild(m.Name);
        
        var costs = Makeable.BuildCosts.GetEnumModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = Makeable.BuildCosts.Get(key) * Amount;
            var have = Makeable.BuildCosts.Get(key) * Fulfilled;
            var str = $"{key.Name}: {have} / {needed}";
            if (key is IIconed i)
            {
                vbox.AddChild(i.Icon.GetLabeledIcon<HBoxContainer>(
                    str, small));
            }
            else
            {
                vbox.CreateLabelAsChild(str);
            }
        }
        
        return vbox;
    }

    public override bool Consolidate(MakeProject next, LogicWriteKey key)
    {
        if (next is not UnitMakeProject p
            || p.Template.Equals(Template) == false)
        {
            return false;
        }

        Amount += p.Amount;
        var before1 = Mathf.FloorToInt(Fulfilled);
        var before2 = Mathf.FloorToInt(next.Fulfilled);
        var before = before1 + before2;
        var after = Mathf.FloorToInt(Fulfilled + next.Fulfilled);
        var diff = after - before;
        var template = Template.Get(key.Data);
        var regime = Regime.Get(key.Data);

        for (var i = 0; i < diff; i++)
        {
            Unit.Create(template, Troops, regime, key);
        }

        Fulfilled += next.Fulfilled;
        return true;
    }

    public override MakeableAttribute GetMakeable(Data d)
    {
        return Makeable;
    }

    public override Icon GetIcon(Data d)
    {
        if (Troops.Contents.Count == 0) return Icon.Blank;
        return Troops.GetEnumModel(d)
            .MaxBy(kvp => kvp.Key.GetPowerPoints() * kvp.Value)
            .Key.Icon;
    }

    public override string Description(Data d)
    {
        return Template.Get(d).Name;
    }

    public float PowerPoints(Data d)
    {
        return Troops.GetEnumModel(d).Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
    }
}