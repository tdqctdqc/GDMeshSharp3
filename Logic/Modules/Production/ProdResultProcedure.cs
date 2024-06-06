
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
            var makeQueue = result.Regime.Get(key.Data).MakeQueue;
            foreach (var (making, amtMade) in result.Made)
            {
                var remainingToMake = amtMade;
                while (remainingToMake > 0f
                       && makeQueue.Queue
                               .FirstOrDefault(p => p.Making.RefId == making.RefId)
                           is MakeProject p)
                {
                    var increment = Mathf.Min(remainingToMake, p.Amount - p.Fulfilled);
                    p.Increment(increment, key);
                    remainingToMake -= increment;
                    if (p.Fulfilled >= p.Amount)
                    {
                        p.Finish(key);
                        makeQueue.Queue.Remove(p);
                    }
                }
            }
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}