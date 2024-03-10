
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public abstract class ConstructionPriority : SolverPriority<BuildingModel>
{
    public ConstructionPriority(string name, 
        Func<Data, Regime, float> getWeight) 
        : base(name, 
            d => d.Models.GetModels<BuildingModel>().Values, 
            getWeight)
    {
    }



    protected override void SetCalcData(Regime r, Data d)
    {
        
    }

    protected override void SetConstraints(Solver solver, Regime r,
        Dictionary<BuildingModel, Variable> projVars, Data data)
    {
        solver.SetBuildingLaborConstraint(Mathf.FloorToInt(Account.Labor), projVars);
        solver.SetItemsConstraints(data, Account.Items, projVars);
        solver.SetConstructCapConstraint( 
            Mathf.FloorToInt(Account.Models.Get(data.Models.Flows.ConstructionCap)), projVars);
        solver.SetBuildingSlotConstraints(r, projVars, data);
    }

    protected override void SetWishlistConstraints(Solver solver, Regime r,
        Dictionary<BuildingModel, Variable> projVars, Data data, 
        BudgetPool pool, float proportion)
    {
        var availConstructCap = Mathf.FloorToInt(pool.AvailModels.Get(data.Models.Flows.ConstructionCap) * proportion);
        var availLabor = Mathf.FloorToInt(pool.AvailLabor * proportion);
        solver.SetBuildingLaborConstraint(availLabor, projVars);
        solver.SetConstructCapConstraint(availConstructCap, projVars);
        solver.SetBuildingSlotConstraints(r, projVars, data);
    }

    protected override void Complete(Regime r, 
        Dictionary<BuildingModel, int> toBuild, LogicWriteKey key)
    {
        var currConstruction = key.Data.Infrastructure.CurrentConstruction;
        var availPolys = r.GetPolys(key.Data);
        
        foreach (var kvp in toBuild)
        {
            var building = kvp.Key;
            int labor = 0;
            if (building.HasComponent<Workplace>())
            {
                labor = building.GetComponent<Workplace>().TotalLaborReq();
            }
            var num = kvp.Value;
            if (num == 0) continue;
            for (var i = 0; i < num; i++)
            {
                MapPolygon poly = null;
                poly = availPolys
                    .FirstOrDefault(p => p.PolyBuildingSlots[building.BuildingType] > 0);
                if (poly == null) continue;
                var slots = poly.PolyBuildingSlots.AvailableSlots[building.BuildingType];
                if (slots.Count() == 0) continue;
                
                var pos = slots.First();
                var proc = StartConstructionProcedure.Construct(
                    building.MakeRef<BuildingModel>(),
                    pos,
                    r.MakeRef(),
                    key.Data
                );
                key.SendMessage(proc);
                var buildCosts = building.Makeable.ItemCosts.GetEnumerableModel(key.Data);
                foreach (var cost in buildCosts)
                {
                    Account.UseItem(cost.Key, cost.Value);
                }
                Account.UseFlow(key.Data.Models.Flows.ConstructionCap, building.ConstructionCapPerTick);
                Account.UseLabor(labor);
            }
        }
    }
}