

using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class MakeReserveTroopsPriority : SolverPriority<Troop>
{
    private Dictionary<TroopType, float> _needed;
    private Regime _regime;
    public MakeReserveTroopsPriority(Regime r) 
            : base("Make Reserve Troops")
    {
        _regime = r;
        _needed = new Dictionary<TroopType, float>();
    }

    protected override string GetName(Troop t, Data d)
    {
        return t.Name;
    }

    protected override float Utility(Troop t, Data d)
    {
        return t.GetPowerPoints();
    }

    protected override bool Relevant(Troop t, Data d)
    {
        return _needed.ContainsKey(t.TroopType);
    }

    protected override void SetCalcData(Regime r, Data d)
    {
        _needed.Clear();
        var units = r.GetUnits(d).ToArray();
        var desired = IdCount<TroopType>.Construct();
        var reserveRatio = .2f;
        var templateCounts = IdCount<UnitTemplate>.Construct();
        foreach (var unit in units)
        {
            templateCounts.Add(unit.Template.RefId, 1);
        }
        foreach (var (template, num) in templateCounts
                     .GetEnumEntity(d))
        {
            foreach (var (troop, amt) in template.Troops.GetEnumModel(d))
            {
                desired.Add(troop, amt * num * reserveRatio);
            }
        }
        
        foreach (var (troop, amt) in desired.GetEnumModel(d))
        {
            var stock = r.Stock.Stock.Get(troop);
            var diff = amt - stock;
            
            //'double counting' this w/ reinforcement priority?
            var producing = r.MakeQueue.Queue.OfType<ModelMakeProject>()
                .Where(p => p.Model.Get(d) == troop)
                .Sum(p => p.Amount);
            diff -= producing;
            if (diff > 0f)
            {
                _needed.AddOrSum(troop, diff);
            }
        }
    }

    protected override void SetConstraints(Solver solver, 
        Regime r, BudgetPool pool, 
        Dictionary<Troop, Variable> projVars, Data data)
    {
        solver.SetBuildCostConstraints(data, pool, projVars);
        solver.SetConstraints(projVars, t => (t.TroopType, 1f),
            _needed, data);
    }

    protected override Dictionary<IModel, float> GetCosts
        (Dictionary<Troop, float> toBuild, Data d)
    {
        var res = new Dictionary<IModel, float>();

        foreach (var (troop, amt) in toBuild)
        {
            var cost = troop.Makeable.BuildCosts;
            foreach (var (model, modelAmt) in cost.GetEnumModel(d))
            {
                res.AddOrSum(model, modelAmt * amt);
            }
        }

        return res;
    }

    protected override IEnumerable<Troop> GetAll(Data d)
    {
        return d.Models.GetModels<Troop>()
            .Where(t => _regime.HasPrereqs(t));
    }

    protected override void Complete(BudgetPool pool, 
        Regime r, Dictionary<Troop, float> toBuild, 
        LogicWriteKey key)
    {
        CompleteModel(pool, r, toBuild, key);
    }
}