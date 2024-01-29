
using System;
using System.Collections.Generic;
using Godot;

public class CombatGraph
{
    private CombatCalculator _combat;
    private Dictionary<int, ICombatGraphNode> _nodesById;
    private Dictionary<Vector2I, List<ICombatGraphEdge>> _edgesByEdgeId;
    private Dictionary<ICombatGraphEdge, (ICombatGraphNode, ICombatGraphNode)> _nodesByEdge;
    private Dictionary<ICombatGraphNode, List<ICombatGraphEdge>> _edgesByNode;

    public CombatGraph(CombatCalculator combat)
    {
        _combat = combat;
        _nodesById = new Dictionary<int, ICombatGraphNode>();
        _edgesByEdgeId = new Dictionary<Vector2I, List<ICombatGraphEdge>>();
        _edgesByNode = new Dictionary<ICombatGraphNode, List<ICombatGraphEdge>>();
        _nodesByEdge = new Dictionary<ICombatGraphEdge, (ICombatGraphNode, ICombatGraphNode)>();
    }

    public bool HasNode(ICombatGraphNode n)
    {
        return _edgesByNode.ContainsKey(n);
    }
    public void AddNode(ICombatGraphNode n)
    {
        if (_edgesByNode.ContainsKey(n)) return;
        _nodesById.Add(n.Id, n);
        _edgesByNode.Add(n, new List<ICombatGraphEdge>());
    }

    public IReadOnlyList<ICombatGraphEdge> GetEdgesBetween(
        ICombatGraphNode n1,
        ICombatGraphNode n2)
    {
        return _edgesByEdgeId[n1.GetIdEdgeKey(n2)];
    }

    public IReadOnlyList<ICombatGraphEdge> GetNodeEdges
        (ICombatGraphNode n)
    {
        AddNode(n);
        return _edgesByNode[n];
    }
    public void AddEdge(ICombatGraphEdge edge, Data d)
    {
        AddNode(edge.Node1);
        AddNode(edge.Node2);
        var edgeId = edge.Node1.GetIdEdgeKey(edge.Node2);
        _edgesByEdgeId.GetOrAdd(edgeId, e => new List<ICombatGraphEdge>())
            .Add(edge);
        _edgesByNode[edge.Node1].Add(edge);
        _edgesByNode[edge.Node2].Add(edge);
        _nodesByEdge.Add(edge, (edge.Node1, edge.Node2));
    }
    public void CalculateCombat(Data d)
    {
        Do((e, combat) 
            => e.CalculateCombat(combat, d));
    }
    public void EnactDirectResults(LogicWriteKey key)
    {
        Do((e, combat) 
            => e.DirectResults(combat, key));
    }
    public void EnactInvoluntaryResults(LogicWriteKey key)
    {
        Do((e, combat) 
            => e.InvoluntaryResults(combat, key));
    }
    public void EnactVoluntaryResults(LogicWriteKey key)
    {
        Do((e, combat) =>
        {
            if (e.Suppressed(combat, key.Data)) return;
            e.VoluntaryResults(combat, key);
        });
    }
    private void Do(Action<ICombatGraphEdge, CombatCalculator> act)
    {
        foreach (var (edgeId, edges) in _edgesByEdgeId)
        {
            var n1 = _nodesById[edgeId.X];
            var n2 = _nodesById[edgeId.X];
            foreach (var edge in edges)
            {
                act(edge, _combat);
            }
        }
    }
}