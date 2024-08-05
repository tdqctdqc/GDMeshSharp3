
using System.Collections.Generic;
using System.Linq;

public class StrategicContext
{
    public HashSet<Cell> AlliedCells { get; private set; }
    public List<HashSet<Cell>> Unions { get; private set; }

    public StrategicContext(Alliance alliance, Data d)
    {
        var alliedCells = d.Planet.MapAux
            .CellHolder.Cells.Values
            .Where(c => alliance.Members.Contains(c.Controller))
            .ToArray();

        AlliedCells = alliedCells
            .ToHashSet();
        Unions = UnionFind.Find<Cell, HashSet<Cell>>(alliedCells,
            (p, q) => true,
            p => p.GetNeighbors(d))
            .ToList();
    }
}