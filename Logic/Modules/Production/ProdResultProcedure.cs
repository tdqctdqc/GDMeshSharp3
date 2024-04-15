
using System.Collections.Generic;

public class ProdResultProcedure : Procedure
{
    public (ERef<Regime>, RegimeStock, Dictionary<int, int> peepGrowths)[] Stocks { get; private set; }

    public ProdResultProcedure((ERef<Regime>, RegimeStock, Dictionary<int, int>)[] stocks)
    {
        Stocks = stocks;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Stocks.Length; i++)
        {
            var (r, stock, growth) = Stocks[i];
            r.Get(key.Data).SetStock(stock, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}