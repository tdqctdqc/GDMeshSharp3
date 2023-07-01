using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class StartConstructionsOrders : TurnOrderModule
{
    public List<StartConstructionRequest> ConstructionsToStart { get; private set; }
    public static StartConstructionsOrders Construct()
    {
        var c = new StartConstructionsOrders(new List<StartConstructionRequest>());
        return c;
    }
    [SerializationConstructor] private StartConstructionsOrders(List<StartConstructionRequest> constructionsToStart)
    {
        ConstructionsToStart = constructionsToStart;
    }
    public override void WriteToResult(Data data, LogicResults res)
    {
        var newConstructionPoses = new HashSet<PolyTriPosition>();
        for (var i = 0; i < ConstructionsToStart.Count; i++)
        {
            var toStart = ConstructionsToStart[i];
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
                Regime
            );
            res.Procedures.Add(proc);
        }
    }
}
