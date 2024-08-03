
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class MakeUnitPriority : SolverPriority<UnitMakeProject>
{
    public MakeUnitPriority()
            : base("Make Units")
    {
        
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
            || r.GetUnitTemplates(data).Count() == 0)
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
            kvp => unitsByMeta.TryGetValue(templates.MetaTemplates[kvp.Key], out var list)
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
            var constraint = constraints[metaTemplate.Tag];
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

    protected override IEnumerable<UnitMakeProject> GetAll(Regime r, Data d)
    {
        var all = d.HostLogicData.RegimeAis[r].Military.Templates
            .MetaTemplates.Select(m => m.Value.Current.Get(d))
            .Select(t => UnitMakeProject.Construct(r, t, 1,
                d)).ToArray();
        return all;
    }

    protected override void Complete(BudgetPool pool, Regime r, 
        Dictionary<UnitMakeProject, float> toBuild, LogicKey key)
    {
        foreach (var (unitMakeProject, value) in toBuild)
        {
            var amount = Mathf.CeilToInt(value);
            var newProj = UnitMakeProject.Construct(
                r, unitMakeProject.Template.Get(key.Data),
                amount, key.Data);
            var proc = new StartOrConsolidateMakeProject(newProj);
            key.SendMessage(proc);
        }
    }

    public override float GetWeight(Regime r, Data d)
    {
        var baseWeight = 5f;
        var templates = d.HostLogicData.RegimeAis[r].Military.Templates;
        var metas = templates.MetaTemplates;
        var desired = d.HostLogicData.RegimeAis[r].Military.ForceComposition.DesiredAmounts;
        var allUnits = r.GetUnits(d)?.ToArray();
        if (allUnits is null || allUnits.Count() == 0) return baseWeight;
        var unitsByMeta = r.GetUnits(d)
            .SortBy(u => u.Template.Get(d).GetMetaTemplate(d));
        var needed = desired.ToDictionary(kvp => kvp.Key,
            kvp => unitsByMeta.TryGetValue(metas[kvp.Key], out var list)
                ? Mathf.Max(0, kvp.Value - list.Count)
                : kvp.Value);
                
        return baseWeight * needed.Sum(kvp => kvp.Value)
               / allUnits.Count();
    }
}