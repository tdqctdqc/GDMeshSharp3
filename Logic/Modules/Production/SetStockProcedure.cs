
public class SetStockProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public RegimeStock Stock { get; private set; }

    public SetStockProcedure(ERef<Regime> regime, RegimeStock stock)
    {
        Regime = regime;
        Stock = stock;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Regime.Get(key.Data).SetStock(Stock, key);
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(Regime.RefId))
        {
            error = "";
            return true;
        }

        error = "couldn't find regime";
        return false;
    }
}