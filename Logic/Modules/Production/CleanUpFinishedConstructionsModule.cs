using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CleanUpFinishedConstructionsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        LogicWriteKey key)
    {
        var finished = new HashSet<Construction>();
        var clear = ClearFinishedConstructionsProcedure.Construct();
        foreach (var r in key.Data.GetAll<Regime>())
        {
            foreach (var kvp in key.Data.Infrastructure.CurrentConstruction.ByTri)
            {
                if (kvp.Value.TicksLeft < 0)
                {
                    finished.Add(kvp.Value);
                }
            }
        }
        foreach (var c in finished)
        {
            clear.Positions.Add(c.Pos);
            MapBuilding.Create(c.Pos, c.Waypoint, 
                c.Model.Model(key.Data), key);
        }
        key.SendMessage(clear);
    }
}
