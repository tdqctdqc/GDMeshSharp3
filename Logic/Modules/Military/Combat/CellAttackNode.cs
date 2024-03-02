
using System;
using System.Linq;

public class CellAttackNode : ICombatGraphNode
{
    public Cell Cell { get; private set; }
    public CellAttackEdge Edge { get; private set; }
    public int Id { get; }

    public CellAttackNode(Cell cell, int id)
    {
        Cell = cell;
        Id = id;
    }
}