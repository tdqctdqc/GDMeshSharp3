
using System;
using System.Linq;

public class CellAttackNode : ICombatGraphNode
{
    public PolyCell Cell { get; private set; }
    public int Id { get; }

    public CellAttackNode(PolyCell cell, int id)
    {
        Cell = cell;
        Id = id;
    }

    public CellAttackEdge Edge(CombatCalculator combat)
    {
        var edges = combat.Graph.GetEdges(this, Cell);
        if (edges.Count != 1) throw new Exception();
        return (CellAttackEdge)edges.First();
    }

}