
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class ProduceConstructModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var results = key.Data.GetAll<Regime>()
            .AsParallel()
            .Select(r => DoRegime(r, key.Data))
            .ToArray();
    }

    private ProductionResult DoRegime(Regime r, Data d)
    {
        var cells = d.Planet.MapAux.CellHolder
            .Cells.Values.Where(c => c.Controller.RefId == r.Id).ToArray();
        var buildings = cells
            .Where(c => c.HasBuilding(d))
            .Select(c => c.GetBuilding(d));
        
        //first add labor flow
        
        
        
        //last do flow consumptions from units, etc
        
        
        
        return null;
    }
}
