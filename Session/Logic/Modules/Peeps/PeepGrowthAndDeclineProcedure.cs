using System;
using System.Collections.Generic;
using System.Linq;

public class PeepGrowthAndDeclineProcedure : Procedure
{
    public Dictionary<int, int> Growths { get; private set; }
    public PeepGrowthAndDeclineProcedure(Dictionary<int, int> growths)
    {
        Growths = growths;
    }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        DoGrowth(key);
        var tick = key.Data.Tick;
        foreach (var r in key.Data.Society.Regimes.Entities)
        {
            r.History.PeepHistory.Update(tick, r, key);
        }
    }

    private void DoGrowth(ProcedureWriteKey key)
    {
        foreach (var kvp in Growths)
        {
            var peep = key.Data.Society.PolyPeeps[kvp.Key];
            var growth = kvp.Value;
            //todo divide by class
            if (growth < 0)
            {
                peep.ShrinkSize(-growth, key);
            }
            
            if (growth > 0)
            {
                peep.GrowSize(growth, key);
            }
        }
    }
}
