
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
        GD.Print($"starting unit make proj");
    }

    public override void Increment(float amount, 
        ProductionResult result,
        LogicWriteKey key)
    {
        Fulfilled += amount;
        GD.Print($"incrementing unit make proj amount {amount}" +
                 $" to {Fulfilled}");
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
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = (UnitTemplate)Making.Get(d);
        var makeable = (IMakeable)m;
        var vbox = new VBoxContainer();
        var icon = m.GetMaxPowerTroop(d).Icon.GetLabeledIcon<HBoxContainer>(
                $"{m.Name}: {Fulfilled} / {Amount} ",
                size);
        vbox.AddChild(icon);
        

        var costs = makeable.Makeable.BuildCosts.GetEnumerableModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = makeable.Makeable.BuildCosts.Get(key) * Amount;
            var have = makeable.Makeable.BuildCosts.Get(key) * Fulfilled;
            vbox.CreateLabelAsChild($"{key.Name}: { have } / { needed }");
        }
        
        return vbox;
    }

    public UnitTemplate MakingTemplate(Data d)
    {
        return (UnitTemplate)Making.Get(d);
    }
}