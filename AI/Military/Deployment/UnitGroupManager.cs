using System;
using System.Collections.Generic;
using System.Linq;

public class UnitGroupManager
{
    public ERef<Regime> Regime { get; private set; }
    public int NodeId { get; private set; }
    public HashSet<ERef<UnitGroup>> Groups { get; private set; }

    public static UnitGroupManager Construct(ERef<Regime> r, int nodeId)
    {
        return new UnitGroupManager(r, nodeId, new HashSet<ERef<UnitGroup>>());
    }
    public UnitGroupManager(ERef<Regime> regime, int nodeId, HashSet<ERef<UnitGroup>> groups)
    {
        Regime = regime;
        NodeId = nodeId;
        Groups = groups;
    }

    public bool Contains(UnitGroup g)
    {
        return Groups.Contains(g.Id);
    }

    public void AddUnassigned(DeploymentAi ai, UnitGroup g, Data d)
    {
        var node = GetNode(ai);
        node.AddGroup(ai, g, d);
    }
    public void Transfer(
        DeploymentAi ai,
        UnitGroup g, 
        GroupAssignment to, 
        LogicWriteKey key)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        var node = GetNode(ai);
        node.RemoveGroup(ai, g);
        to.AddGroup(ai, g, key.Data);
    }
    public void Add(DeploymentAi ai, UnitGroup g)
    {
        if (Groups.Contains(g.Id)) throw new Exception();
        Groups.Add(g.Id);
        ai.GroupAssignments.Add(g.MakeRef(), GetNode(ai));
    }
    public void Remove(DeploymentAi ai, UnitGroup g)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        var node = GetNode(ai);
        Groups.Remove(g.Id);
        ai.GroupAssignments.Remove(g.MakeRef());
    }

    public IEnumerable<UnitGroup> Get(Data d)
    {
        return Groups.Select(g => g.Entity(d));
    }

    public int Count()
    {
        return Groups.Count;
    }

    private GroupAssignment GetNode(DeploymentAi ai)
    {
        return (GroupAssignment)ai.GetNode(NodeId);
    }

    public void Disband(LogicWriteKey key)
    {
        
    }
}