
using MessagePack;

public class DefaultMakeProject : MakeProject
{
    public static MakeProject Construct<TMakeable>(
        Regime r,
        TMakeable t,
        float amount)
        where TMakeable : class, IModel, IMakeable
    {
        return new DefaultMakeProject(r.MakeRef(),
            ((IModel)t).MakeRef(),
            amount, 0f);
    }
    [SerializationConstructor] protected DefaultMakeProject(ERef<Regime> regime, ModelRef<IModel> making, float amount, float fulfilled) : base(regime, making, amount, fulfilled)
    {
    }

    public override void Start(ProcedureWriteKey key)
    {
        
    }

    public override void Increment(float amount, ProcedureWriteKey key)
    {
        Fulfilled += amount;
        var stock = Regime.Get(key.Data).Stock;
        stock.Stock.Add(Making.RefId, amount);
        stock.Produced.Add(Making.RefId, amount);
    }

    public override void Finish(ProcedureWriteKey key)
    {
        
    }
}