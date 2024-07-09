
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

    public override void Start(ProcedureWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        Fulfilled += BuildTree.Increment(this, regime.Stock, key);
    }

    public override void Increment(float amount, 
        ProductionResult result,
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
            var spent = diff * amt;
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

    public UnitTemplate MakingTemplate(Data d)
    {
        return (UnitTemplate)Making.Get(d);
    }
}