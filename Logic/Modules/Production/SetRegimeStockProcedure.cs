
public class SetRegimeStockProcedure : Procedure
{
    public (ERef<Regime>, RegimeStock)[] Stocks { get; private set; }


    public SetRegimeStockProcedure((ERef<Regime>, RegimeStock)[] stocks)
    {
        Stocks = stocks;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Stocks.Length; i++)
        {
            var (r, stock) = Stocks[i];
            r.Get(key.Data).SetStock(stock, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}