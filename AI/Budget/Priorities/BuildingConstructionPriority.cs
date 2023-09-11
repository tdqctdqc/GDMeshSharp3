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

    public override void Calculate(Regime regime, Data data,
        MajorTurnOrders orders, HashSet<Item> usedItem, HashSet<Flow> usedFlow, ref bool usedLabor)
    {
        usedLabor = false;
        if (Account.Labor <= 0) return;
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        
        SetBuildingLaborConstraint(solver, Mathf.FloorToInt(Account.Labor), projVars);
        SetItemConstraints(solver, data, Account.Items, projVars);
        SetConstructCapConstraint(solver, 
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
        SelectBuildSitesAndAddRequest(regime, data, buildings, orders, usedItem, usedFlow, ref usedLabor);
    }

    public override Dictionary<Item, int> GetWishlist(Regime regime, Data data, 
        int availLabor, int availConstructCap)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        
        SetBuildingLaborConstraint(solver, availLabor, projVars);
        // SetCreditConstraint(solver, data, credit, prices, projVars);
        SetConstructCapConstraint(solver, availConstructCap, projVars);
        SetSlotConstraints(solver, regime, projVars, data);
        
        var success = Solve(solver, projVars);
        if (success == false)
        {
        }
        return projVars.GetCounts(kvp => kvp.Key.BuildCosts, 
            (kvp, i) => Mathf.CeilToInt(i * kvp.Value.SolutionValue()));
    }

    private Solver MakeSolver()
    {
        var solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
        // var solver = Solver.CreateSolver("GLOP");
        if (solver is null)
        {
            throw new Exception("solver null");
        }

        return solver;
    }

    private IEnumerable<BuildingModel> GetRelevantBuildings(Data data)
    {
        return data.Models.GetModels<BuildingModel>().Values.Where(_relevant);
    }

    private Dictionary<BuildingModel, Variable> MakeProjVars(Solver solver, Data data)
    {
        var buildings = GetRelevantBuildings(data);
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
    
    private void SetItemConstraints(Solver solver, Data data, ItemCount budget,
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var items = data.Models.GetModels<Item>().Select(kvp => kvp.Value.Id).ToList();
        var itemNumConstraints = new Dictionary<int, Constraint>();
        items.ForEach(i =>
        {
            var itemConstraint = solver.MakeConstraint(0f, budget.Get(i));
            itemNumConstraints.Add(i, itemConstraint);
        });
        foreach (var kvp in buildingVars)
        {
            var b = kvp.Key;
            var projVar = kvp.Value;
            foreach (var kvp2 in b.BuildCosts)
            {
                var item = kvp2.Key;
                var num = kvp2.Value;
                var itemConstraint = itemNumConstraints[item.Id];
                itemConstraint.SetCoefficient(projVar, num);
            }
        }
    }

    private void SetCreditConstraint(Solver solver, Data data, float credit,
        Dictionary<Item, float> prices, 
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var creditConstraint = solver.MakeConstraint(0f, credit, "Credits");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var projPrice = kvp.Key.BuildCosts
                .Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
        }
    }

    private void SetConstructCapConstraint(Solver solver, int availConstructCap, 
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var constructLaborConstraint = solver.MakeConstraint(0, availConstructCap, "ConstructLabor");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var b = kvp.Key;
            constructLaborConstraint.SetCoefficient(projVar, b.ConstructionCapPerTick);
        }
    }
    private void SetBuildingLaborConstraint(Solver solver, int laborAvail, 
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var buildingLaborConstraint = solver.MakeConstraint(0, laborAvail, "BuildingLabor");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var b = kvp.Key;
            if(b.HasComponent<Workplace>())
            {
                buildingLaborConstraint.SetCoefficient(projVar, b.GetComponent<Workplace>().TotalLaborReq());
            }
        }
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
    private void SelectBuildSitesAndAddRequest(Regime regime, Data data, 
        Dictionary<BuildingModel, int> toBuild,
        MajorTurnOrders orders, HashSet<Item> usedItem, HashSet<Flow> usedFlow, ref bool usedLabor)
    {
        var currConstruction = data.Infrastructure.CurrentConstruction;
        var availPolys = regime.GetPolys(data);
        
        foreach (var kvp in toBuild)
        {
            var building = kvp.Key;
            int labor = 0;
            if (building.HasComponent<Workplace>())
            {
                usedLabor = true;
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
                
                foreach (var cost in building.BuildCosts)
                {
                    Account.Items.Remove(cost.Key, cost.Value);
                    usedItem.Add(cost.Key);
                }
                
                Account.Flows.Remove(data.Models.Flows.ConstructionCap, building.ConstructionCapPerTick);
                
                Account.UseLabor(labor);
            }
        }
    }
}
