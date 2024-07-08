
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class NaturalResource : TradeableItem
{
    protected NaturalResource(string name, Color color, float initialPrice) 
        : base(name, color, initialPrice)
    {
    }
    public HashSet<Cell> GenerateDeposits(Data data)
    {
        var cells = data.Planet.MapAux
            .CellHolder.Cells.Values;
        var deps = new HashSet<Cell>();
        var scores = new Dictionary<MapPolygon, int>();
        foreach (var p in cells)
        {
            var score = GetDepositChance(p, data);
            if (Game.I.Random.Randf() > score) continue;
            deps.Add(p);
        }
        return deps;
    }
    public abstract float GetDepositChance(Cell c, Data d);

}
