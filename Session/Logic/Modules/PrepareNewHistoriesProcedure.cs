using System;
using System.Collections.Generic;
using System.Linq;

public class PrepareNewHistoriesProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        if (key.Data.BaseDomain.GameClock.MajorTurn(key.Data))
        {
            foreach (var regime in key.Data.GetAll<Regime>())
            {
                regime.History.PrepareNewMajorTick(key.Data.BaseDomain.GameClock.Tick, key);
            }
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
