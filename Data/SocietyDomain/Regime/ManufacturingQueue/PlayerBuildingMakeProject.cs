
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
            1f, 0f);
    }
    [SerializationConstructor] protected PlayerBuildingMakeProject(
        ERef<Settlement> settlement,
        ERef<Regime> regime, 
        IdRef making,
        float amount,
        float fulfilled) 
            : base(regime, making, amount, fulfilled)
    {
        Settlement = settlement;
    }

    public override void Start(ProcedureWriteKey key)
    {
        var building = (SettlementBuildingModel)Making.Get(key.Data);
        var settlement = Settlement.Get(key.Data);
        var regime = Regime.Get(key.Data);
        var population = regime.GetPopulation(key.Data);
        var proportion = 1f;
        foreach (var (model, amt) 
                 in building.Makeable.BuildCosts
                     .GetEnumerableModel(key.Data))
        {
            var modelStock = regime.Stock.Stock.Get(model);
            if(modelStock == 0f)
            {
                proportion = 0f;
                break;
            }
            var modelProportion = Mathf.Clamp(modelStock / amt, 0f, 1f);
            proportion = Mathf.Min(proportion, modelProportion);
        }

        if (proportion > 0f)
        {
            Fulfilled = proportion;
            foreach (var (model, amt) in building.Makeable.BuildCosts.GetEnumerableModel(key.Data))
            {
                regime.Stock.Stock.Remove(model, amt * proportion);
                regime.Stock.SingleTimeCosts.Add(model, amt * proportion);
            }
        }
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
        

        var costs = makeable.Makeable.BuildCosts.GetEnumerableModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = makeable.Makeable.BuildCosts.Get(key) * Amount;
            var have = makeable.Makeable.BuildCosts.Get(key) * Fulfilled;
            vbox.CreateLabelAsChild($"{key.Name}: { have } / { needed }");
        }
        
        return vbox;
    }
}