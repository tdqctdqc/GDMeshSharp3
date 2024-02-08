using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using MessagePack;

public class DeploymentAi
{
    public Regime Regime { get; private set; }
    private Dictionary<int, IDeploymentNode> _nodesById;
    public Dictionary<ERef<UnitGroup>, GroupAssignment> GroupAssignments { get; private set; }
    public DeploymentRoot Root { get; private set; }
    public IdRecycler DeploymentTreeIds { get; private set; }
    private Data _data;
    public static DeploymentAi Construct(Regime r, Data d)
    {
        var ai = new DeploymentAi(r, new IdRecycler(), d);
        var root = DeploymentRoot.Construct(r, ai, d);
        ai.Root = root;
        ai.AddNode(root);
        
        d.SubscribeForDestruction<UnitGroup>(n =>
        {
            var group = (UnitGroup)n.Entity;
            if (ai.GroupAssignments.TryGetValue(group.MakeRef(), out var assgn))
            {
                assgn.RemoveGroup(ai, group);
            }
        });
        return ai;
    }
    private DeploymentAi(Regime r, IdRecycler deploymentTreeIds, 
        Data d)
    {
        _data = d;
        Regime = r;
        DeploymentTreeIds = deploymentTreeIds;
        _nodesById = new Dictionary<int, IDeploymentNode>();
        GroupAssignments = new Dictionary<ERef<UnitGroup>, GroupAssignment>();
    }
    public void Calculate(Regime regime, LogicWriteKey key, MinorTurnOrders orders)
    {
        Root.MakeTheaters(this, key);
        Root.GrabUnassignedGroups(key);
        Root.ShiftGroups(this, key);
        Root.AdjustWithin(this, key);
        Root.GiveOrders(this, key);
    }
    public IDeploymentNode GetNode(int id)
    {
        return _nodesById[id];
    }
    public void AddNode(IDeploymentNode n)
    {
        if (_nodesById.ContainsKey(n.Id))
        {
            throw new Exception($"{n.GetType().Name} already in nodes");
        }
        _nodesById.Add(n.Id, n);
        if (n is GroupAssignment ga)
        {
            foreach (var g in ga.Groups)
            {
                GroupAssignments.Add(g, ga);
            }
        }
        foreach (var c in n.Children())
        {
            AddNode(c);
        }
    }
    public void RemoveNode(int id, LogicWriteKey key)
    {
        var node = _nodesById[id];
        foreach (var c in node.Children())
        {
            RemoveNode(c.Id, key);
        }
        if (node is GroupAssignment ga)
        {
            foreach (var g in ga.Groups)
            {
                GroupAssignments.Remove(g);
            }
        }
        _nodesById.Remove(id);
    }

}