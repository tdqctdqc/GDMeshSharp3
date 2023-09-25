using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class BuildingConstructionPriority : BudgetPriority
{
    private Func<BuildingModel, bool> _relevant;
    private Func<BuildingModel, int> _utility;
    public BuildingConstructionPriority(
        string name,
        Func<BuildingModel, bool> relevant, 
        Func<BuildingModel, int> utility,
        Func<Data, Regime, float> getWeight) 
        : base(name, getWeight)
    {
        _relevant = relevant;
        _utility = utility;
    }

    public override void Calculate(Regime regime, Data data, MajorTurnOrders orders)
    {
        if (Account.Labor <= 0) return;
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        
        solver.SetBuildingLaborConstraint(Mathf.FloorToInt(Account.Labor), projVars);
        solver.SetItemConstraints(data, Account.Items, projVars);
        solver.SetConstructCapConstraint( 
            Mathf.FloorToInt(Account.Flows[data.Models.Flows.ConstructionCap]), projVars);
        SetSlotConstraints(solver, regime, projVars, data);
        
        var success = Solve(solver, projVars);
        if (success == false)
        {
            foreach (var kvp in Account.Items.Contents)
            {
                var item = (Item) data.Models[kvp.Key];
                var q = kvp.Value;
            }
        }
        
        var buildings = projVars.ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
        Complete(regime, data, buildings, orders);
    }

    public override Dictionary<Item, int> GetWishlist(Regime regime, Data data, 
        BudgetPool pool, float proportion)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        var availConstructCap = Mathf.FloorToInt(pool.AvailFlows[data.Models.Flows.ConstructionCap] * proportion);
        var availLabor = Mathf.FloorToInt(pool.AvailLabor * proportion);
        solver.SetBuildingLaborConstraint(availLabor, projVars);
        solver.SetConstructCapConstraint(availConstructCap, projVars);
        SetSlotConstraints(solver, regime, projVars, data);
        
        var success = Solve(solver, projVars);
        if (success == false)
        {
        }
        return projVars.GetCounts(
            kvp => kvp.Key.Attributes.Get<MakeableAttribute>().ItemCosts, 
            (kvp, i) => Mathf.CeilToInt(i * kvp.Value.SolutionValue()));
    }
    private Dictionary<BuildingModel, Variable> MakeProjVars(Solver solver, Data data)
    {
        var buildings = data.Models.GetModels<BuildingModel>().Values.Where(_relevant);
        return buildings.Select(b =>
        {
            var projVar = solver.MakeIntVar(0, 1000, b.Name);
            return new KeyValuePair<BuildingModel, Variable>(b, projVar);
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    private bool Solve(Solver solver, Dictionary<BuildingModel, Variable> projVars)
    {
        var objective = solver.Objective();
        objective.SetMaximization();
        
        foreach (var kvp in projVars)
        {
            var b = kvp.Key;
            var projVar = projVars[b];
            var benefit = _utility(b);
            objective.SetCoefficient(projVar, benefit);
        }
        var status = solver.Solve();
        return status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE;
    }

    
    

    private void SetSlotConstraints(Solver solver, Regime regime, Dictionary<BuildingModel, Variable> buildingVars, Data data)
    {
        var slotConstraints = new Dictionary<BuildingType, Constraint>();
        var slotTypes = buildingVars.Select(kvp => kvp.Key.BuildingType).Distinct();
        
        foreach (var slotType in slotTypes)
        {
            var slots = regime.GetPolys(data).Select(p => p.PolyBuildingSlots[slotType]).Sum();
            var slotConstraint = solver.MakeConstraint(0, slots, slotType.ToString());
            slotConstraints.Add(slotType, slotConstraint);
        }
        foreach (var kvp in buildingVars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            var slotConstraint = slotConstraints[b.BuildingType];
            slotConstraint.SetCoefficient(projVar, 1);
        }
    }
    private void Complete(Regime regime, Data data, 
        Dictionary<BuildingModel, int> toBuild,
        MajorTurnOrders orders)
    {
        var currConstruction = data.Infrastructure.CurrentConstruction;
        var availPolys = regime.GetPolys(data);
        
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

                var buildCosts = building.Attributes.Get<MakeableAttribute>().ItemCosts;
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
