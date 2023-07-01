using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public class ProdBuildingConstructBudgetPriority : BudgetPriority
{
    public Item ProducedItem { get; private set; }
    public ProdBuildingConstructBudgetPriority(Item producedItem, Func<Data, Regime, float> getWeight) 
        : base(getWeight)
    {
        ProducedItem = producedItem;
    }

    public override void Calculate(Regime regime, Data data, ItemWallet budget, Dictionary<Item, float> prices,
        int credit, int availLabor, MajorTurnOrders orders)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        
        SetBuildingLaborConstraint(solver, availLabor, projVars);
        SetItemConstraints(solver, data, budget, projVars);
        SetCreditConstraint(solver, data, credit, prices, projVars);
        SetConstructLaborConstraint(solver, availLabor, projVars);
        SetSlotConstraints(solver, regime, projVars);
        
        var buildings = Solve(solver, projVars, regime, data, prices, credit, availLabor);
        SelectBuildSites(regime, data, buildings, budget, orders);
    }

    public override Dictionary<Item, int> GetItemWishlist(Regime regime, Data data, 
        Dictionary<Item, float> prices, int credit, int availLabor)
    {
        var solver = MakeSolver();
        var projVars = MakeProjVars(solver, data);
        
        SetBuildingLaborConstraint(solver, availLabor, projVars);
        SetCreditConstraint(solver, data, credit, prices, projVars);
        SetConstructLaborConstraint(solver, availLabor, projVars);
        SetSlotConstraints(solver, regime, projVars);
        
        Solve(solver, projVars, regime, data, 
            prices, credit, availLabor);
        return projVars.GetCounts(kvp => kvp.Key.BuildCosts, (kvp, i) => Mathf.CeilToInt(i * kvp.Value.SolutionValue()));
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

    private IEnumerable<ProductionBuildingModel> GetRelevantBuildings(Data data)
    {
        return data.Models.Buildings.Models.Values
            .SelectWhereOfType<BuildingModel, ProductionBuildingModel>()
            .Where(pb => pb.ProdItem == ProducedItem);
    }

    private Dictionary<BuildingModel, Variable> MakeProjVars(Solver solver, Data data)
    {
        var buildings = GetRelevantBuildings(data);
        return buildings.Select(b =>
        {
            var projVar = solver.MakeIntVar(0, int.MaxValue, b.Name);
            return new KeyValuePair<BuildingModel, Variable>(b, projVar);
        }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    private Dictionary<BuildingModel, int> Solve(Solver solver, 
        Dictionary<BuildingModel, Variable> projVars,
        Regime regime, Data data,
        Dictionary<Item, float> prices,
        float credit, int laborAvail)
    {
        var objective = solver.Objective();
        objective.SetMaximization();
        
        foreach (var kvp in projVars)
        {
            var b = (ProductionBuildingModel)kvp.Key;
            var projVar = projVars[b];
            var projPrice = b.BuildCosts.Sum(kvp => prices[kvp.Key] * kvp.Value);
            var benefit = b.ProductionCap;
            objective.SetCoefficient(projVar, benefit);
        }
        var status = solver.Solve();
        if (status != Solver.ResultStatus.OPTIMAL
            && status != Solver.ResultStatus.FEASIBLE)
        {
            //throw new Exception();
            GD.Print(status);
        }

        return projVars.ToDictionary(v => v.Key, v => (int)v.Value.SolutionValue());
    }
    private void SetItemConstraints(Solver solver, Data data, ItemWallet budget,
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var items = data.Models.Items.Models.Select(kvp => kvp.Value.Id).ToList();
        var itemNumConstraints = new Dictionary<int, Constraint>();
        items.ForEach(i =>
        {
            var itemConstraint = solver.MakeConstraint(0f, budget[i]);
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
            var projPrice = kvp.Key.BuildCosts.Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
        }
    }

    private void SetConstructLaborConstraint(Solver solver, int laborAvail, 
        Dictionary<BuildingModel, Variable> buildingVars)
    {
        var constructLaborConstraint = solver.MakeConstraint(0, laborAvail, "ConstructLabor");
        foreach (var kvp in buildingVars)
        {
            var projVar = kvp.Value;
            var b = kvp.Key;
            constructLaborConstraint.SetCoefficient(projVar, b.LaborPerTickToBuild);
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
            if(b is WorkBuildingModel w)
            {
                buildingLaborConstraint.SetCoefficient(projVar, w.TotalLaborReq());
            }
        }
    }

    private void SetSlotConstraints(Solver solver, Regime regime, Dictionary<BuildingModel, Variable> buildingVars)
    {
        var slotConstraints = new Dictionary<BuildingType, Constraint>();
        var slotTypes = buildingVars.Select(kvp => kvp.Key.BuildingType).Distinct();
        
        foreach (var slotType in slotTypes)
        {
            var slots = regime.Polygons.Select(p => p.PolyBuildingSlots[slotType]).Sum();
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
    private void SelectBuildSites(Regime regime, Data data, Dictionary<BuildingModel, int> toBuild, 
        ItemWallet budget, MajorTurnOrders orders)
    {
        var currConstruction = data.Society.CurrentConstruction;
        var availPolys = regime.Polygons;
        
        //sort buildings by type then assign polys from that
        foreach (var kvp in toBuild)
        {
            var building = kvp.Key;
            var num = kvp.Value;
            for (var i = 0; i < num; i++)
            {
                MapPolygon poly = null;
                poly = availPolys
                    .FirstOrDefault(p => p.PolyBuildingSlots[building.BuildingType] > 0);
                if (poly == null) continue;
                orders.StartConstructions.ConstructionsToStart.Add(StartConstructionRequest.Construct(building, poly));
            }
        }
    }
}
