using System;
using System.Collections.Generic;
using System.Linq;

public class UnitGroupManager
{
    public ERef<Regime> Regime { get; private set; }
    public int NodeId { get; private set; }
    public HashSet<ERef<UnitGroup>> Groups { get; private set; }
    public bool Contains(UnitGroup g)
    {
        return Groups.Contains(g.Id);
    }
    public void Transfer(UnitGroup g, 
        DeploymentLeaf to, 
        LogicWriteKey key)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        var node = GetNode(key.Data);
        node.ClearGroupFromData(g, key);
        Remove(g, key);
        to.AddGroupToData(g, key);
        to.Groups.Add(g, key);
    }
    private void Add(UnitGroup g, LogicWriteKey key)
    {
        if (Groups.Contains(g.Id)) throw new Exception();
        Groups.Add(g.Id);
    }
    private void Remove(UnitGroup g, LogicWriteKey key)
    {
        if (Groups.Contains(g.Id) == false) throw new Exception();
        Groups.Remove(g.Id);
    }

    public IEnumerable<UnitGroup> Get(Data d)
    {
        return Groups.Select(g => g.Entity(d));
    }

    public int Count()
    {
        return Groups.Count;
    }

    private DeploymentLeaf GetNode(Data d)
    {
        return (DeploymentLeaf)d.HostLogicData.RegimeAis.Dic[Regime.Entity(d)]
            .Military.Deployment.GetNode(NodeId);
    }
}