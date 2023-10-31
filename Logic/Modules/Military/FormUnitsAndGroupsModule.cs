
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FormUnitsAndGroupsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, LogicWriteKey key)
    {

        orders.ForEach(o =>
        {
            FormUnits((MajorTurnOrders)o, key.Data, key.SendMessage, key);
            FormGroups((MajorTurnOrders)o, key.Data, key.SendMessage, key);
        });
    }

    private void FormUnits(MajorTurnOrders orders, Data data, 
        Action<Message> sendMessage, LogicWriteKey key)
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
        sendMessage(useTroops);
    }

    private void FormGroups(MajorTurnOrders orders, Data data, 
        Action<Message> sendMessage, LogicWriteKey key)
    {
        foreach (var newGroupUnits in orders.Military.NewGroupUnits)
        {
            UnitGroup.Create(orders.Regime.Entity(data),
                newGroupUnits, key);
        }
    }
}