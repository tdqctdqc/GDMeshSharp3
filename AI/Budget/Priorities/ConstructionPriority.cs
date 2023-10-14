
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class ConstructionPriority : SolverPriority<BuildingModel>
{
    public ConstructionPriority(string name, 
        Func<Data, Regime, float> getWeight, 
        Func<BuildingModel, bool> relevant, Func<BuildingModel, float> utility) 
        : base(name, d => d.Models.GetModels<BuildingModel>().Values, 
            getWeight, relevant, utility)
    {
    }


    protected override void SetConstraints(Solver solver, Regime r,
        Dictionary<BuildingModel, Variable> projVars, Data data)
    {
        solver.SetBuildingLaborConstraint(Mathf.FloorToInt(Account.Labor), projVars);
        solver.SetItemsConstraints(data, Account.Items, projVars);
        solver.SetConstructCapConstraint( 
            Mathf.FloorToInt(Account.Flows.Get(data.Models.Flows.ConstructionCap)), projVars);
        solver.SetBuildingSlotConstraints(r, projVars, data);
    }

    protected override void SetWishlistConstraints(Solver solver, Regime r,
        Dictionary<BuildingModel, Variable> projVars, Data data, 
        BudgetPool pool, float proportion)
    {
        var availConstructCap = Mathf.FloorToInt(pool.AvailFlows.Get(data.Models.Flows.ConstructionCap) * proportion);
        var availLabor = Mathf.FloorToInt(pool.AvailLabor * proportion);
        solver.SetBuildingLaborConstraint(availLabor, projVars);
        solver.SetConstructCapConstraint(availConstructCap, projVars);
        solver.SetBuildingSlotConstraints(r, projVars, data);
    }

    protected override void Complete(Regime r, MajorTurnOrders orders, 
        Dictionary<BuildingModel, int> toBuild, Data data)
    {
        var currConstruction = data.Infrastructure.CurrentConstruction;
        var availPolys = r.GetPolys(data);
        
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
                orders.StartConstructions.ConstructionsToStart
                    .Add(StartConstructionRequest.Construct(building, poly));
                
                var buildCosts = building.Makeable.ItemCosts.GetEnumerableModel(data);
                foreach (var cost in buildCosts)
                {
                    Account.UseItem(cost.Key, cost.Value);
                }
                Account.UseFlow(data.Models.Flows.ConstructionCap, building.ConstructionCapPerTick);
                Account.UseLabor(labor);
            }
        }
    }
}