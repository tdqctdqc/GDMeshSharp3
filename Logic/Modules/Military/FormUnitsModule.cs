
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FormUnitsModule : LogicModule
{
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        GD.Print("DOING FORM UNITS MODULE");
        orders.ForEach(o => FormUnits((MajorTurnOrders)o, data, res));
        return res;
    }

    private void FormUnits(MajorTurnOrders orders, Data data, LogicResults res)
    {
        var regime = orders.Regime.Entity(data);
        var capitalPos = regime.Capital.Entity(data).Center;
        var useTroops = RegimeUseTroopsProcedure.Construct(regime);
        var regimeTroops = regime.TroopReserve;

        var availTroops =
            regime.TroopReserve.GetEnumerableModel(data)
                .ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value);
        foreach (var id in orders.MilitaryOrders.UnitTemplatesToBuild)
        {
            var template = data.Get<UnitTemplate>(id);
            var troopCosts = template
                .TroopCounts.GetEnumerableModel(data);
            var build = true;
            foreach (var kvp in troopCosts)
            {
                var troop = kvp.Key;
                var num = kvp.Value;
                if(regimeTroops.Get(troop) < num)
                {
                    build = false;
                    break;
                }
            }
            if (build == false) continue;
            foreach (var kvp in troopCosts)
            {
                var troop = kvp.Key;
                var num = kvp.Value;
                useTroops.UsageByTroopId.AddOrSum(troop.Id, (int)num);
                availTroops[troop] -= num;
            }
            res.CreateEntities.Add(k => Unit.Create(template, regime, capitalPos, k));
        }
        res.Messages.Add(useTroops);
    }
}