
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class CellHolder : Entity
{
    public Dictionary<int, Cell> Cells { get; private set; }
    public Dictionary<Vector2I, int> Lefts { get; private set; }
    public Dictionary<Vector2I, int> Rights { get; private set; }
    public static CellHolder Create(Dictionary<int, Cell> cells, GenKey key)
    {


        var (lefts, rights) 
            = MakeLeftsAndRights(cells, key.Data);
        
        var e = new CellHolder(cells, 
            lefts, rights,
            key.Data.IdDispenser.TakeId());
        key.Create(e);
        return e;
    }
    [SerializationConstructor] private CellHolder(
        Dictionary<int, Cell> cells, 
        Dictionary<Vector2I, int> lefts, 
        Dictionary<Vector2I, int> rights,
        int id) : base(id)
    {
        Lefts = lefts;
        Rights = rights;
        Cells = cells;
    }

    private static (Dictionary<Vector2I, int> lefts,
        Dictionary<Vector2I, int> rights)
        MakeLeftsAndRights(Dictionary<int, Cell> cells, Data d)
    {
        var (lefts, rights) =
            (new Dictionary<Vector2I, int>(),
                new Dictionary<Vector2I, int>());
        
        foreach (var cell in cells.Values)
        {
            if (cell is not IPolyCell) continue;
            for (var i = 0; i < cell.Neighbors.Count; i++)
            {
                var nId = cell.Neighbors[i];
                if (nId < cell.Id) continue;
                var nCell = cells[cell.Neighbors[i]];
                if (nCell is not IPolyCell) continue;
                
                Cell left = null;
                Cell right = null;
        
                var nfAxis = cell.GetCenter()
                    .Offset(nCell.GetCenter(), d);
                var sharedNs = cell.Neighbors
                    .Intersect(nCell.Neighbors)
                    .Distinct()
                    .Select(i => cells[i])
                    .Where(n => n is IPolyCell);

                int iter = 0;
                foreach (var sharedN in sharedNs)
                {
                    iter++;
                    if (iter > 2) throw new Exception();
                    var nAxis = cell.GetCenter()
                        .Offset(sharedN.GetCenter(), d);
                    var onLeft = nfAxis.GetCCWAngleTo(nAxis) < Mathf.Pi;
                    if (onLeft)
                    {
                        if (left != null) throw new Exception();
                        left = sharedN;
                    }
                    else
                    {
                        if (right != null) throw new Exception();
                        right = sharedN;
                    }
                }

                var leftId = left is not null ? left.Id : -1;
                var rightId = right is not null ? right.Id : -1;
                var key = cell.GetIdEdgeKey(nCell);
                lefts.Add(key, leftId);
                rights.Add(key, rightId);
            }
        }

        return (lefts, rights);
    }
    public override void CleanUp(IWriteKey key)
    {
        
    }
}