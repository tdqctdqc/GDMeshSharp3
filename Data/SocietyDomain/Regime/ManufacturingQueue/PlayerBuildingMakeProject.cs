
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