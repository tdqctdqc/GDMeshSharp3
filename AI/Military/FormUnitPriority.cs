using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class FormUnitPriority 
{
    public FormUnitPriority(string name, 
        Func<Data, IEnumerable<UnitTemplate>> getAll, 
        Func<Data, Regime, float> getWeight) 
    {
    }

    protected float Utility(UnitTemplate t)
    {
        return 1f;
    }

    protected bool Relevant(UnitTemplate t, Data d)
    {
        return true;
    }

    protected void SetCalcData(Regime r, Data d)
    {
        
    }

    protected void Complete(
        BudgetPool pool,
        Regime r, 
        Dictionary<UnitTemplate, int> toBuild, 
        LogicWriteKey key)
    {
        var useTroops = RegimeUseTroopsProcedure.Construct(r);
        var capitalPoly = r.Capital.Get(key.Data);
        var pos = (Vector2I)capitalPoly.Center;
        var cell = 
            capitalPoly.GetCells(key.Data).First(c => c is LandCell);
        var deployPolyCell = r.GetCells(key.Data).Where(goodDeployCell).First();

        bool goodDeployCell(Cell c)
        {
            return c.Controller.RefId == r.Id;
        }
        
        var unitPos = new MapPos(deployPolyCell.Id, (-1, 0f));
        
        foreach (var (template, num) in toBuild)
        {
            useTroops.AddTroopCosts(template, num, key.Data);
            for (var i = 0; i < num; i++)
            {
                Unit.Create(template, r, unitPos.Copy(), key);
            }
        }
    }
}