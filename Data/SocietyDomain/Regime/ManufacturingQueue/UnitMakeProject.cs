
using Godot;
using MessagePack;

public class UnitMakeProject : MakeProject
{
    public static UnitMakeProject Construct(Regime r,
        UnitTemplate template,
        int amount)
    {
        return new UnitMakeProject(r.MakeRef(), template.MakeRef(),
            amount, 0f, -1);
    }
    [SerializationConstructor] private UnitMakeProject(ERef<Regime> regime, 
        IdRef making, float amount, float fulfilled, int id) 
            : base(regime, making, amount, fulfilled, id)
    {
    }

    public override void Start(LogicWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        var before = Mathf.FloorToInt(Fulfilled);
        var increment = BuildTree.Increment(this, regime.Stock, key);
        if (increment == 0f) return;
        Fulfilled += increment;
        var after = Mathf.FloorToInt(Fulfilled);
        var diff = after - before;
        for (var i = 0; i < diff; i++)
        {
            Unit.Create((UnitTemplate)Making.Get(key.Data),
                Regime.Get(key.Data), key);
        }
        var setStock = new SetStockProcedure(regime.MakeRef(),
            regime.Stock);
        key.SendMessage(setStock);
    }

    public override void Increment(float amount, 
        RegimeStock stock,
        LogicWriteKey key)
    {
        var before = Mathf.FloorToInt(Fulfilled);
        Fulfilled += amount;
        var after = Mathf.FloorToInt(Fulfilled);
        var made = after - before;
        for (var i = 0; i < made; i++)
        {
            Unit.Create((UnitTemplate)Making.Get(key.Data),
                Regime.Get(key.Data), key);
        }
    }

    public override void Finish(LogicWriteKey key)
    {
        
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        var making = MakingTemplate(key.Data);
        var stock = Regime.Get(key.Data).Stock;
        var numMade = Mathf.FloorToInt(Fulfilled);
        var diff = Fulfilled - numMade;
        foreach (var (model, amt) in making.Makeable.BuildCosts.GetEnumModel(key.Data))
        {
            var spent = numMade * amt;
            stock.Stock.Add(model, spent);
        }
    }

    public override Control GetDisplay(Data d)
    {
        var large = Game.I.Client.Settings.LargeIconSize.Value;
        var small = Game.I.Client.Settings.SmallIconSize.Value;
        var m = MakingTemplate(d);
        var makeable = (IMakeable)m;
        var vbox = new VBoxContainer();
        var icon = m.GetMaxPowerTroop(d).Icon.GetLabeledIcon<HBoxContainer>(
                $"{m.Name}: {Fulfilled} / {Amount} ",
                large);
        vbox.AddChild(icon);
        vbox.CreateLabelAsChild(m.Name);
        
        var costs = makeable.Makeable.BuildCosts.GetEnumModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = makeable.Makeable.BuildCosts.Get(key) * Amount;
            var have = makeable.Makeable.BuildCosts.Get(key) * Fulfilled;
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
            || p.MakingTemplate(key.Data) != MakingTemplate(key.Data))
        {
            return false;
        }

        Amount += p.Amount;
        var before1 = Mathf.FloorToInt(Fulfilled);
        var before2 = Mathf.FloorToInt(next.Fulfilled);
        var before = before1 + before2;
        var after = Mathf.FloorToInt(Fulfilled + next.Fulfilled);
        var diff = after - before;
        var template = MakingTemplate(key.Data);
        var regime = Regime.Get(key.Data);

        for (var i = 0; i < diff; i++)
        {
            Unit.Create(template, regime, key);
        }

        Fulfilled += next.Fulfilled;
        return true;
    }

    public UnitTemplate MakingTemplate(Data d)
    {
        return (UnitTemplate)Making.Get(d);
    }
}