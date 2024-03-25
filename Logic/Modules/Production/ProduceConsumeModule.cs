
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class ProduceConsumeModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var results = key.Data.GetAll<Regime>()
            .AsParallel()
            .Select(r => (r.MakeRef(), DoRegime(r, key.Data)))
            .ToArray();
        var proc = new SetRegimeStockProcedure(results);
        key.SendMessage(proc);
    }

    private RegimeStock DoRegime(Regime r, Data d)
    {
        var res = RegimeStock.Construct();
        foreach (var (id, amt) in r.Stock.Stock.Contents.ToList())
        {
            var model = d.Models.GetModel<IModel>(id);
            if (model is Flow)
            {
                r.Stock.Stock.Set(model, 0f);
            }
        }
        var cells = d.Planet.MapAux.CellHolder
            .Cells.Values.Where(c => c.Controller.RefId == r.Id).ToArray();
        
        var totalPop = cells
            .Sum(c => c.GetPeep(d).Size);
        var labor = d.Models.Flows.Labor;
        r.Stock.Stock.Set(labor, totalPop);
        
        DoBuildingProds(r, d, res);
        TroopMaintenance(r, d, res);

        res.Stock.Add(r.Stock.Stock);
        
        return res;
    }

    private static void TroopMaintenance(Regime r, Data d, RegimeStock res)
    {
        var units = r.GetUnits(d);
        var milCap = d.Models.Flows.MilitaryCap;
        var milCapCost = 0f;
        foreach (var unit in units)
        {
            foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(d))
            {
                milCapCost += troop.Makeable.MaintainCosts.Get(milCap) * amt;
            }
        }

        var milCapAvail = r.Stock.Stock.Get(milCap);
        res.RecurringCosts.Add(milCap, milCapCost);
        r.Stock.Stock.Remove(milCap, Mathf.Min(milCapAvail, milCapCost));
    }

    private static void DoBuildingProds(Regime r, Data d,
        RegimeStock res)
    {
        var prodBuildings = r.GetCells(d)
            .Where(c => c.HasBuilding(d))
            .Select(c => c.GetBuilding(d).Model.Get(d))
            .Where(b => b.HasComponent<BuildingProd>())
            .GetCounts();

        var buildingProdQueue = new PriorityQueue<
            (BuildingModel, int num, float satisfied), float>();
        foreach (var (b, num) in prodBuildings)
        {
            buildingProdQueue.Enqueue((b, num, 0f), 1f);
        }

        int itersSinceLastProd = 0;
        while (buildingProdQueue.TryDequeue(out var e,
                   out var p))
        {
            var (model, num, satisfied) = e;
            var prod = model.GetComponent<BuildingProd>();
            var satisfactionIncrement = prod.Inputs.GetEnumerableModel(d)
                .Min(kvp => r.Stock.Stock.Get(kvp.Key) / (kvp.Value * num));
            satisfactionIncrement = Mathf.Clamp(satisfactionIncrement, 0f, 1f - satisfied);

            if (satisfactionIncrement > 0f)
            {
                itersSinceLastProd = 0;
                foreach (var (inputModel, inputAmt)
                         in prod.Inputs.GetEnumerableModel(d))
                {
                    r.Stock.Stock.Remove(inputModel,
                        inputAmt * num * satisfactionIncrement);
                    res.RecurringCosts.Add(inputModel,
                        inputAmt * num * satisfactionIncrement);
                }

                foreach (var (outputModel, outputAmt)
                         in prod.Outputs.GetEnumerableModel(d))
                {
                    r.Stock.Stock.Add(outputModel,
                        outputAmt * num * satisfactionIncrement);
                    res.Produced.Add(outputModel,
                        outputAmt * num * satisfactionIncrement);
                }
            }
            else
            {
                itersSinceLastProd++;
                if (itersSinceLastProd > buildingProdQueue.Count)
                {
                    break;
                }
            }

            satisfied += satisfactionIncrement;
            if (satisfied < 1f)
            {
                buildingProdQueue.Enqueue((model, num, satisfied), p - 1f);
            }
        }
    }
    
}
