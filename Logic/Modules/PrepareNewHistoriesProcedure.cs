using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PrepareNewHistoriesProcedure : Procedure
{
    public PrepareNewHistoriesProcedure()
    {
        GD.Print("constructing new hist proc");
    }
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
