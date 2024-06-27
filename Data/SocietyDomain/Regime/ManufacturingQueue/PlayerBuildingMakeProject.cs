
using Godot;
using MessagePack;

public class PlayerBuildingMakeProject : MakeProject
{
    public ERef<Settlement> Settlement { get; private set; }

    public static PlayerBuildingMakeProject Construct(
        Settlement settlement,
        Regime regime,
        SettlementBuildingModel making)
    {
        return new PlayerBuildingMakeProject(
            settlement.MakeRef(), regime.MakeRef(),
            making.MakeRef<IModel>(),
            1f, 0f, -1);
    }
    [SerializationConstructor] protected PlayerBuildingMakeProject(
        ERef<Settlement> settlement,
        ERef<Regime> regime, 
        IdRef making,
        float amount,
        float fulfilled, int id) 
            : base(regime, making, amount, fulfilled, id)
    {
        Settlement = settlement;
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
        var building = (SettlementBuildingModel)Making.Get(key.Data);
        var settlement = Settlement.Get(key.Data);
        var regime = Regime.Get(key.Data);
        var proc = new AddBuildingProcedure(Settlement, building.MakeRef());
        key.SendMessage(proc);
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        var making = MakingBuilding(key.Data);
        var stock = Regime.Get(key.Data).Stock;
        foreach (var (model, amt) in making.Makeable.BuildCosts.GetEnumModel(key.Data))
        {
            var spent = Fulfilled * amt;
            stock.Stock.Add(model, spent);
        }
    }

    public SettlementBuildingModel MakingBuilding(Data d)
    {
        return (SettlementBuildingModel)Making.Get(d);
    }

    public override Control GetDisplay(Data d)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = (IModel)Making.Get(d);
        var makeable = (IMakeable)m;
        var vbox = new VBoxContainer();
        if (m is IIconed i)
        {
            var icon = i.Icon.GetLabeledIcon<HBoxContainer>(
                $"{m.Name}: {Fulfilled} / {Amount} ",
                size);
            vbox.AddChild(icon);
        }
        else
        {
            vbox.CreateLabelAsChild(m.Name);
        }
        

        var costs = makeable.Makeable.BuildCosts.GetEnumModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = makeable.Makeable.BuildCosts.Get(key) * Amount;
            var have = makeable.Makeable.BuildCosts.Get(key) * Fulfilled;
            vbox.CreateLabelAsChild($"{key.Name}: { have } / { needed }");
        }
        
        return vbox;
    }
}