
using Godot;
using MessagePack;

public class UnitMakeProject : MakeProject
{
    public static UnitMakeProject Construct(Regime r,
        UnitTemplate template)
    {
        return new UnitMakeProject(r.MakeRef(), template.MakeRef(),
            1f, 0f, -1);
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
        Fulfilled += amount;
    }

    public override void Finish(LogicWriteKey key)
    {
        Unit.Create((UnitTemplate)Making.Get(key.Data),
            Regime.Get(key.Data), key);
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        var making = MakingTemplate(key.Data);
        var stock = Regime.Get(key.Data).Stock;
        foreach (var (model, amt) in making.Makeable.BuildCosts.GetEnumerableModel(key.Data))
        {
            var spent = Fulfilled * amt;
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
        
        var costs = makeable.Makeable.BuildCosts.GetEnumerableModel(d);
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