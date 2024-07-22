
using System.Collections.Generic;
using System.Linq;
using Godot;
using Priority_Queue;

public class AiPlaceBuildingsModule : LogicModule
{
    
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {
        var procs = new List<Procedure>();
        foreach (var regime in key.Data.GetAll<Regime>())
        {
            if (regime.IsPlayerRegime(key.Data)) continue;
            var settlements = regime.GetSettlements(key.Data);
            var queue = new SimplePriorityQueue<Settlement, float>();
            foreach (var s in settlements)
            {
                queue.Enqueue(s, -s.Cell.Get(key.Data).GetLaborCounts(key.Data).free);
            }
            
            foreach (var (model, value) in regime.Stock.Stock
                         .GetEnumModel(key.Data))
            {
                if (model is SettlementBuilding sb)
                {
                    var floor = Mathf.FloorToInt(value);
                    var labor = sb.Labor.TotalLabor();
                    for (var i = 0; i < floor; i++)
                    {
                        var s = queue.First;
                        var priority = queue.GetPriority(s);
                        queue.UpdatePriority(s, priority + labor);
                        procs.Add(new AddSettlementBuildingProcedure(s.MakeRef(),
                            sb.MakeRef()));
                        regime.Stock.Stock.Remove(model, 1);
                    }
                }
            }

            var resourceDeposits = regime.GetCells(key.Data)
                .Where(c => c.HasResourceDeposit(key.Data))
                .Select(c => c.GetResourceDeposit(key.Data))
                .Where(rd => rd.Extraction.IsEmpty())
                .ToArray();

            var byNatResource = resourceDeposits.SortBy(rd => (NaturalResource)rd.Item.Get(key.Data));
            var rdFreeLabor = resourceDeposits.ToDictionary(rd => rd,
                rd => rd.Cell.Get(key.Data).GetLaborCounts(key.Data).free);
            
            foreach (var (model, value) in regime.Stock.Stock
                         .GetEnumModel(key.Data))
            {
                if (model is ResourceExtractionBuilding rb)
                {
                    var floor = Mathf.FloorToInt(value);
                    var labor = rb.Labor.TotalLabor();
                    var resource = rb.Resource(key.Data);
                    if (byNatResource.ContainsKey(resource) == false) continue;
                    var list = byNatResource[resource];
                    for (var i = 0; i < floor; i++)
                    {
                        if (list.Count == 0)
                        {
                            break;
                        }
                        var deposit = list.MaxBy(rd => rdFreeLabor[rd]);
                        procs.Add(new AddResourceExtractionProcedure(rb.MakeRef(),
                            deposit.MakeRef()));
                        regime.Stock.Stock.Remove(model, 1);
                    }
                }
            }
            
            procs.Add(new SetStockProcedure(regime.MakeRef(),
                regime.Stock));
        }
        key.SendMessage(new AggregateProcedure(procs.ToArray()));
    }
}