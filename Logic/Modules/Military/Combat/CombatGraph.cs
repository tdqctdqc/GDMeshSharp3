
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

    public void AddNode(ICombatGraphNode n)
    {
        if (_edgesByNode.ContainsKey(n)) return;
        _nodesById.Add(n.Id, n);
        _edgesByNode.Add(n, new List<ICombatGraphEdge>());
    }

    public IReadOnlyList<ICombatGraphEdge> GetEdges(
        ICombatGraphNode n1,
        ICombatGraphNode n2)
    {
        return _edgesByEdgeId[n1.GetIdEdgeKey(n2)];
    }
    public void AddEdge(ICombatGraphNode n1, 
        ICombatGraphNode n2,
        ICombatGraphEdge edge, Data d)
    {
        AddNode(n1);
        AddNode(n2);
        var edgeId = n1.GetIdEdgeKey(n2);
        _edgesByEdgeId[edgeId].Add(edge);
        _edgesByNode[n1].Add(edge);
        _edgesByNode[n2].Add(edge);
        _nodesByEdge.Add(edge, (n1, n2));
        edge.PrepareGraph(_combat, n1, n2, d);
    }
    public void Calculate(Data d)
    {
        Do((e, combat, n1, n2) 
            => e.Calculate(combat, n1, n2, d));
    }
    public void DirectResults(Data d)
    {
        Do((e, combat, n1, n2) 
            => e.DirectResults(combat, n1, n2, d));
    }
    public void InvoluntaryResults(Data d)
    {
        Do((e, combat, n1, n2) 
            => e.InvoluntaryResults(combat, n1, n2, d));
    }
    public void VoluntaryResults(Data d)
    {
        Do((e, combat, n1, n2) 
            => e.VoluntaryResults(combat, n1, n2, d));
    }
    private void Do(Action<ICombatGraphEdge, CombatCalculator, 
        ICombatGraphNode, ICombatGraphNode> act)
    {
        foreach (var (edgeId, edges) in _edgesByEdgeId)
        {
            var n1 = _nodesById[edgeId.X];
            var n2 = _nodesById[edgeId.X];
            foreach (var edge in edges)
            {
                act(edge, _combat, n1, n2);
            }
        }
    }
}