
using System;
using System.Linq;

public class CellAttackNode : ICombatGraphNode
{
    public PolyCell Cell { get; private set; }
    public CellAttackEdge Edge { get; private set; }
    public int Id { get; }

    public CellAttackNode(PolyCell cell, int id)
    {
        Cell = cell;
        Id = id;
    }
}