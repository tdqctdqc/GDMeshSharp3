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
            foreach (var kvp in key.Data.Infrastructure.CurrentConstruction.ByPolyCell)
            {
                if (kvp.Value.TicksLeft < 0)
                {
                    finished.Add(kvp.Value);
                }
            }
        }
        foreach (var c in finished)
        {
            var cell = PlanetDomainExt.GetPolyCell(c.PolyCellId, key.Data);
            clear.PolyCellIds.Add(c.PolyCellId);
            MapBuilding.Create(cell, ((LandCell)cell).Polygon.Entity(key.Data),
                c.Model.Model(key.Data), key);
        }
        key.SendMessage(clear);
    }
}
