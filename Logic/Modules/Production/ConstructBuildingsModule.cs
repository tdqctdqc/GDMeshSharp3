using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ConstructBuildingsModule : LogicModule
{
    public override void Calculate(List<RegimeTurnOrders> orders, 
        Data data, Action<Message> sendMessage)
    {
        var key = new LogicWriteKey(sendMessage, data);
        var finished = new HashSet<Construction>();
        var clear = ClearFinishedConstructionsProcedure.Construct();
        foreach (var r in data.GetAll<Regime>())
        {
            foreach (var kvp in data.Infrastructure.CurrentConstruction.ByTri)
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
                c.Model.Model(data), key);
        }
        
        for (var i = 0; i < orders.Count; i++)
        {
            var order = orders[i];
            if (order is MajorTurnOrders m == false) throw new Exception();
            var newConstructionPoses = new HashSet<PolyTriPosition>();
            for (var j = 0; j < m.StartConstructions.ConstructionsToStart.Count; j++)
            {
                var toStart = m.StartConstructions.ConstructionsToStart[j];
                var poly = (MapPolygon) data[toStart.PolyId];
                var building = (BuildingModel) data.Models[toStart.BuildingModelId];
                var slots = poly.PolyBuildingSlots.AvailableSlots[building.BuildingType]
                    .Where(pt => newConstructionPoses.Contains(pt) == false);
                if (slots.Count() == 0) continue;
                var pos = slots.First();
                newConstructionPoses.Add(pos);
                
                var proc = StartConstructionProcedure.Construct(
                    building.MakeRef<BuildingModel>(),
                    pos,
                    poly.GetCenterWaypoint(data).Id,
                    order.Regime,
                    data
                );
                sendMessage(proc);
            }
        }
        
        sendMessage(clear);
    }
}
