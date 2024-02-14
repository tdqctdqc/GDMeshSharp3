
using System.Linq;

public class UnoccupiedAssignment : GroupAssignment
{
    public PolyCell Cell { get; private set; }
    public UnoccupiedAssignment(PolyCell cell, DeploymentBranch parent, 
        DeploymentAi ai, LogicWriteKey key) : base(parent, ai, key)
    {
        Cell = cell;
    }
    protected override void RemoveGroupFromData(DeploymentAi ai, UnitGroup g)
    {
        
    }
    protected override void AddGroupToData(DeploymentAi ai, UnitGroup g, Data d)
    {
        
    }
    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }
    public override void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return Cell;
    }

    public override UnitGroup PullGroup(DeploymentAi ai, LogicWriteKey key)
    {
        if (Groups.Count == 0) return null;
        var g = Groups.First();
        Groups.Remove(g);
        return g;
    }
}