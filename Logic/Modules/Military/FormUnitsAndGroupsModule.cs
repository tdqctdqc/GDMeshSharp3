
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FormUnitsAndGroupsModule : LogicModule
{
    public override LogicResults Calculate(List<RegimeTurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var key = new LogicWriteKey(data, res);
        orders.ForEach(o =>
        {
            FormUnits((MajorTurnOrders)o, data, key);
            FormGroups((MajorTurnOrders)o, data, key);
        });
        return res;
    }

    private void FormUnits(MajorTurnOrders orders, Data data, 
        LogicWriteKey key)
    {
        var regime = orders.Regime.Entity(data);
        var capitalPos = regime.Capital.Entity(data).Center;
        var useTroops = RegimeUseTroopsProcedure.Construct(regime);
        
        var availTroops =
            regime.Military.TroopReserve.GetEnumerableModel(data)
                .ToDictionary(kvp => kvp.Key,
                    kvp => kvp.Value);
        foreach (var id in orders.Military.UnitTemplatesToForm)
        {
            var template = data.Get<UnitTemplate>(id);
            var troopCosts = template
                .TroopCounts.GetEnumerableModel(data);
            var build = true;
            foreach (var kvp in troopCosts)
            {
                var troop = kvp.Key;
                var num = kvp.Value;
                if(availTroops[troop] < num)
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
                useTroops.UsageByTroopId.AddOrSum(troop.Id, 
                    num);
                availTroops[troop] -= num;
            }
            Unit.Create(template, regime, capitalPos, key);
        }
        key.Results.Messages.Add(useTroops);
    }

    private void FormGroups(MajorTurnOrders orders, Data data, 
        LogicWriteKey key)
    {
        foreach (var newGroupUnits in orders.Military.NewGroupUnits)
        {
            UnitGroup.Create(orders.Regime.Entity(data),
                newGroupUnits, key);
        }
    }
}