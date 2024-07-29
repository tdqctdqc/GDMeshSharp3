
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class MakeUnitPriority : SolverPriority<UnitMakeProject>
{
    private Regime _regime;
    public MakeUnitPriority(Regime regime, Data d)
            : base("Make Units")
    {
        _regime = regime;
    }

    protected override string GetName(UnitMakeProject t, Data d)
    {
        return t.Template.Get(d).Name;
    }

    protected override float Utility(UnitMakeProject t, Data d)
    {
        return t.PowerPoints(d);
    }

    protected override bool Relevant(UnitMakeProject t, Data d)
    {
        return true;
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        
    }

    protected override void SetConstraints(Solver solver, 
        Regime r, BudgetPool pool, 
        Dictionary<UnitMakeProject, Variable> projVars, Data data)
    {
        if (r.GetUnitTemplates(data) is null
            || r.GetUnitTemplates(data).Count() == 0
            || r.GetUnits(data) is null)
        {
            return;
        }
        var templates = data.HostLogicData.RegimeAis[r].Military
            .Templates;
        var desired = data.HostLogicData.RegimeAis[r].Military.ForceComposition.DesiredAmounts;
        var unitsByMeta = 
            r.GetUnits(data)
            .SortBy(u =>
            {
                var m = u.Template.Get(data).GetMetaTemplate(data);
                if(m is null)
                {
                    templates.CategorizeTemplate(u.Template.Get(data), data);
                }
                m = u.Template.Get(data).GetMetaTemplate(data);
                if (m is null) throw new Exception();
                return m;
            });
        var needed = desired.ToDictionary(kvp => kvp.Key,
            kvp => unitsByMeta.TryGetValue(kvp.Key, out var list)
                ? Mathf.Max(0, kvp.Value - list.Count)
                : kvp.Value);
        
        var constraints = needed.ToDictionary(
            kvp => kvp.Key,
            kvp => solver.MakeConstraint(0, kvp.Value)
        );
        
        foreach (var (project, v) in projVars)
        {
            var metaTemplate = project.Template.Get(data)
                .GetMetaTemplate(data);
            if (metaTemplate is null)
            {
                throw new Exception($"{project.Template.Get(data).Name} has no meta");
            }
            var constraint = constraints[metaTemplate];
            constraint.SetCoefficient(v, 1);
        }
        solver.SetBuildCostConstraints(
            data, pool, projVars, 
            p => p.MakeableBase);
    }

    protected override Dictionary<IModel, float> GetCosts(
        Dictionary<UnitMakeProject, float> toBuild, Data d)
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

    protected override IEnumerable<UnitMakeProject> GetAll(Data d)
    {
        return d.HostLogicData.RegimeAis[_regime].Military.Templates
            .MetaTemplates.Select(m => m.Current.Get(d))
            .Select(t => UnitMakeProject.Construct(_regime, t, 1,
                d));
    }

    protected override void Complete(BudgetPool pool, Regime r, 
        Dictionary<UnitMakeProject, float> toBuild, LogicKey key)
    {
        foreach (var (unitMakeProject, value) in toBuild)
        {
            var newProj = UnitMakeProject.Construct(
                r, unitMakeProject.Template.Get(key.Data),
                (int)value, key.Data);
            var proc = new StartOrConsolidateMakeProject(unitMakeProject);
            key.SendMessage(proc);
        }
    }
}