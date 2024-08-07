
using System.Collections.Generic;
using System.Linq;

public class StrategicContext
{
    public HashSet<Cell> AlliedCells { get; private set; }
    public List<HashSet<Cell>> Unions { get; private set; }
    public HashSet<Cell> Gained { get; private set; }
    public List<HashSet<Cell>> GainedUnions { get; private set; }

    public HashSet<Cell> Lost { get; private set; }
    public List<HashSet<Cell>> LostUnions { get; private set; }
    public HashSet<Cell> Stable { get; private set; }
    public List<HashSet<Cell>> StableUnions { get; private set; }
    public StrategicContext(Alliance alliance, 
        HashSet<Cell> prev,
        Data d)
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
        
        
        
        Gained = AlliedCells.Except(prev).ToHashSet();
        if (prev.Count == 0)
        {
            Gained = new HashSet<Cell>();
        }
        GainedUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            Gained, (c, d) => true, c => c.GetNeighbors(d));
        
        Lost = prev.Except(AlliedCells).ToHashSet();
        if (prev.Count == 0)
        {
            Lost = new HashSet<Cell>();
        }
        LostUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            Lost, (c, d) => true, c => c.GetNeighbors(d));

        Stable = AlliedCells.Intersect(prev).ToHashSet();
        if (prev.Count == 0)
        {
            Stable = AlliedCells.ToHashSet();
        }
        StableUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            Stable, (c, d) => true, c => c.GetNeighbors(d));
    }
}