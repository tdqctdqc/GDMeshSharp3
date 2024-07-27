
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class UpgradeTroopsPriority : SolverPriority<TroopUpgradeProject>
{
    private Regime _regime;
    private Dictionary<Troop, int> _needed;
    private Dictionary<TroopType, Troop> _best;
    public UpgradeTroopsPriority(Regime regime) 
        : base("Upgrade Troops")
    {
        _regime = regime;
    }

    protected override IEnumerable<TroopUpgradeProject> GetAll(Data d)
    {
        return _needed.Select(kvp => TroopUpgradeProject.Construct(_regime, kvp.Key, _best[kvp.Key.TroopType],
            1, d));
    }
    protected override string GetName(TroopUpgradeProject t, Data d)
    {
        return t.Description(d);
    }

    protected override float Utility(TroopUpgradeProject t, Data d)
    {
        return Mathf.Max(0f, t.To.Get(d).GetPowerPoints() - t.From.Get(d).GetPowerPoints());
    }

    protected override bool Relevant(TroopUpgradeProject t, Data d)
    {
        return true;
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        var troops = _regime.GetAllTroopAmounts(d);

        _best = d.Models.GetModels<TroopType>()
            .ToDictionary(tt => tt, tt => r.Military.GetBestTroopOfType(tt, d));
        _needed = new Dictionary<Troop, int>();
        foreach (var (troop, value) in troops)
        {
            if (_best[troop.TroopType] != troop)
            {
                _needed.Add(troop, Mathf.CeilToInt(value));
            }
        }
    }

    protected override void SetConstraints(Solver solver, 
        Regime r, BudgetPool pool, 
        Dictionary<TroopUpgradeProject, Variable> projVars, 
        Data data)
    {
        foreach (var (proj, projVar) in projVars)
        {
            projVar.SetLb(0);
            projVar.SetUb(_needed[proj.From.Get(data)]);
        }
        solver.SetBuildCostConstraints(data, pool,
            projVars);
    }

    protected override Dictionary<IModel, float> GetCosts(Dictionary<TroopUpgradeProject, float> toBuild, Data d)
    {
        var res = new Dictionary<IModel, float>();
        
        foreach (var (makeProj, value) in toBuild)
        {
            foreach (var (item, amount) in makeProj.Makeable.BuildCosts.GetEnumModel(d))
            {
                res.AddOrSum(item, amount * value);
            }
        }

        return res;
    }

    protected override void Complete(BudgetPool pool, Regime r, 
        Dictionary<TroopUpgradeProject, float> toBuild, 
        LogicWriteKey key)
    {
        foreach (var (project, value) in toBuild)
        {
            var from = project.From.Get(key.Data);
            var remaining = value;
            foreach (var unit in _regime.GetUnits(key.Data))
            {
                if (remaining <= 0f) break;
                if (unit.Troops.Contents.ContainsKey(project.From.RefId) == false)
                {
                    continue;
                }

                var unitAmt = unit.Troops.Get(from);
                unitAmt = Mathf.Min(unitAmt, remaining);
                remaining -= unitAmt;
                var newProj = TroopUpgradeProject.Construct(
                    r, project.From.Get(key.Data),
                    project.To.Get(key.Data),
                    unitAmt, key.Data,
                    unit);
                var proc = new StartOrConsolidateMakeProject(newProj);
                key.SendMessage(proc);
            }

            if (remaining > 0f)
            {
                var inStock = r.Stock.Stock.Get(from);
                var amt = MathF.Min(inStock, remaining);
                var newProj = TroopUpgradeProject.Construct(
                    r, project.From.Get(key.Data),
                    project.To.Get(key.Data),
                    amt, key.Data);
                var proc = new StartOrConsolidateMakeProject(newProj);
                key.SendMessage(proc);
            }
            
            
            
        }
    }
}