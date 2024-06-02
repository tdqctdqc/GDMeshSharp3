
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CombatGraph
{
    private CombatCalculator _combat;
    public Dictionary<Cell, CellCombatNode> CellCombatNodes { get; private set; }
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
        CellCombatNodes = new Dictionary<Cell, CellCombatNode>();
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

    public void RemoveNode(ICombatGraphNode n)
    {
        var edges = _edgesByNode[n];
        foreach (var edge in edges)
        {
            var (n1, n2) = _nodesByEdge[edge];
            var key = n1.GetIdEdgeKey(n2);
            var other = n == n1 ? n2 : n1;
            _edgesByNode[n2].Remove(edge);
            _edgesByEdgeId.Remove(key);
            _nodesByEdge.Remove(edge);
        }

        _edgesByNode.Remove(n);

        if (n is CellCombatNode c)
        {
            CellCombatNodes.Remove(c.Cell);
        }

        _nodesById.Remove(n.Id);
     }


    public IReadOnlyList<ICombatGraphEdge> GetEdgesBetween(
        ICombatGraphNode n1,
        ICombatGraphNode n2)
    {
        if (_edgesByEdgeId.TryGetValue(n1.GetIdEdgeKey(n2), out var edges))
        {
            return edges;
        }

        return null;
    }

    public IReadOnlyList<ICombatGraphEdge> GetNodeEdges
        (ICombatGraphNode n)
    {
        AddNode(n);
        return _edgesByNode[n];
    }

    public void AddEdge(ICombatGraphNode node1,
        ICombatGraphNode node2,
        ICombatGraphEdge edge, Data d)
    {
        AddNode(node1);
        AddNode(node2);
        var edgeId = node1.GetIdEdgeKey(node2);
        _edgesByEdgeId.GetOrAdd(edgeId, e => new List<ICombatGraphEdge>())
            .Add(edge);
        _edgesByNode[node1].Add(edge);
        _edgesByNode[node2].Add(edge);
        _nodesByEdge.Add(edge, (node1, node2));
    }

    public void DistributeResources(Data d)
    {
        Do((e, combat) 
            => e.DistributeResources(combat, d));
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
            e.VoluntaryResults(combat, key);
        });
    }
    private void Do(Action<ICombatGraphNode, CombatCalculator> act)
    {
        foreach (var (id, node) in _nodesById)
        {
            // var n1 = _nodesById[edgeId.X];
            // var n2 = _nodesById[edgeId.X];
            act(node, _combat);
        }
    }
}