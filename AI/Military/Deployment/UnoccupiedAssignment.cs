
using System;
using System.Collections.Generic;
using System.Linq;

public class UnoccupiedAssignment : GroupAssignment
{
    public CellRef Cell { get; private set; }

    public UnoccupiedAssignment(int id, DeploymentBranch parent, 
        ERef<Alliance> alliance, HashSet<ERef<Army>> groups, 
        CellRef cell) : base(id, parent, alliance, groups)
    {
        Cell = cell;
    }

    protected override void RemoveGroupFromData(DeploymentAi ai, Army g)
    {
        
    }
    protected override void AddGroupToData(DeploymentAi ai, Army g, Data d)
    {
        
    }
    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }
    public override void GiveOrders(DeploymentAi ai, LogicKey key)
    {
        
    }

    public override float Suitability(Army g, Data d)
    {
        return 1f;
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Cell.Get(d);
    }

    public override Army PullGroup(DeploymentAi ai, 
        Func<Army, float> suitability, 
        LogicKey key)
    {
        if (Groups.Count == 0) return null;
        var g = Groups.MaxBy(v => suitability(v.Get(key.Data)));
        Groups.Remove(g);
        return g.Get(key.Data);
    }
}