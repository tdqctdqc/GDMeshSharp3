
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class CombatGraph
{
    private CombatCalculator _combat;
    public Dictionary<CellRef, int> CellDefNodes { get; private set; }
    public Dictionary<Vector2I, int> CellAtkNodes { get; private set; }
    public Dictionary<int, ICombatGraphNode> NodesById { get; private set; }
    public Dictionary<ICombatGraphNode, List<int>> NodeNeighbors { get; private set; }
    public IEnumerable<ICombatGraphNode> GetNodes() => NodesById.Values;
    
    public CombatGraph()
    {
        NodesById = new Dictionary<int, ICombatGraphNode>();
        NodeNeighbors = new Dictionary<ICombatGraphNode, List<int>>();
        CellDefNodes = new Dictionary<CellRef, int>();
        CellAtkNodes = new Dictionary<Vector2I, int>();
    }

    [SerializationConstructor] private CombatGraph( 
        Dictionary<int, ICombatGraphNode> nodesById, 
        Dictionary<ICombatGraphNode, List<int>> nodeNeighbors, 
        Dictionary<CellRef, int> cellDefNodes, 
        Dictionary<Vector2I, int> cellAtkNodes)
    {
        NodesById = nodesById;
        NodeNeighbors = nodeNeighbors;
        CellDefNodes = cellDefNodes;
        CellAtkNodes = cellAtkNodes;
    }

    public void AddNode(ICombatGraphNode n)
    {
        if (NodesById.ContainsKey(n.Id)) return;
        NodesById.Add(n.Id, n);
        NodeNeighbors.Add(n, new List<int>());
    }

    public void RemoveNode(ICombatGraphNode node)
    {
        var neighbors = NodeNeighbors[node];
        foreach (var nId in neighbors)
        {
            var neighbor = NodesById[nId];
            NodeNeighbors[neighbor].Remove(node.Id);
        }

        NodesById.Remove(node.Id);
        NodeNeighbors.Remove(node);
        if (node is CellDefenseNode d)
        {
            CellDefNodes.Remove(d.Cell);
        }

        if (node is CellAttackNode a)
        {
            var key = new Vector2I(a.From.RefId, a.Target.RefId);
            CellAtkNodes.Remove(key);
        }
     }

    

    public IEnumerable<ICombatGraphNode> GetNeighbors(ICombatGraphNode node)
    {
        var ns = NodeNeighbors[node];
        for (var i = 0; i < ns.Count; i++)
        {
            yield return NodesById[ns[i]];
        }
    }

    public void AddEdge(ICombatGraphNode node1,
        ICombatGraphNode node2)
    {
        AddNode(node1);
        AddNode(node2);
        NodeNeighbors[node1].Add(node2.Id);
        NodeNeighbors[node2].Add(node1.Id);
    }
    private void Do(Action<ICombatGraphNode, CombatCalculator> act)
    {
        foreach (var (id, node) in NodesById.ToArray())
        {
            act(node, _combat);
        }
    }
}