
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ProdResultProcedure : Procedure
{
    public ProductionResult[] Results { get; private set; }

    public ProdResultProcedure(ProductionResult[] results)
    {
        Results = results;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Results.Length; i++)
        {
            var result = Results[i];
            result.Regime.Get(key.Data).SetStock(result.Stock, key);
            result.Regime.Get(key.Data).MakeQueue.SetQueue(result.MakeQueue, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}