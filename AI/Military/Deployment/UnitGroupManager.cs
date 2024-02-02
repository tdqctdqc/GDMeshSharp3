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

    public void AddUnassigned(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        Add(ai, g, key);
    }
    public void Transfer(
        DeploymentAi ai,
        UnitGroup g, 
        GroupAssignment to, 
        LogicWriteKey key)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        var node = GetNode(ai, key.Data);
        node.ClearGroupFromData(ai, g, key);
        Remove(ai, g, key);
        to.AddGroup(ai, g, key);
        // to.Groups.Add(ai, g, key);
    }
    public void Add(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        if (Groups.Contains(g.Id)) throw new Exception();
        Groups.Add(g.Id);
        ai.GroupAssignments.Add(g.MakeRef(), GetNode(ai, key.Data));
    }
    private void Remove(DeploymentAi ai, UnitGroup g, LogicWriteKey key)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
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

    private GroupAssignment GetNode(DeploymentAi ai, Data d)
    {
        return (GroupAssignment)ai.GetNode(NodeId);
    }

    public void Disband(LogicWriteKey key)
    {
        
    }
}