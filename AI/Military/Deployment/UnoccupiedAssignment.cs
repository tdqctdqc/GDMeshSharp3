
using System;
using System.Linq;

public class UnoccupiedAssignment : GroupAssignment
{
    public Cell Cell { get; private set; }
    public UnoccupiedAssignment(Cell cell, DeploymentBranch parent, 
        DeploymentAi ai, LogicWriteKey key) : base(parent, ai, key)
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
    public override void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        
    }

    public override float Suitability(Army g, Data d)
    {
        return 1f;
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Cell;
    }

    public override Army PullGroup(DeploymentAi ai, 
        Func<Army, float> suitability, 
        LogicWriteKey key)
    {
        if (Groups.Count == 0) return null;
        var g = Groups.MaxBy(suitability);
        Groups.Remove(g);
        return g;
    }
}