
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
        ModelRef<IModel> making,
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

    public override void Increment(float amount, ProcedureWriteKey key)
    {
        Fulfilled += amount;
    }

    public override void Finish(ProcedureWriteKey key)
    {
        var building = (SettlementBuildingModel)Making.Get(key.Data);
        var settlement = Settlement.Get(key.Data);
        var regime = Regime.Get(key.Data);
        settlement.Buildings.Add(building, 1f);
        regime.Stock.Produced.Add(Making.RefId, 1f);
    }
}