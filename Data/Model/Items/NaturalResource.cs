
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
            var score = GetDepositScore(p, data);
            var chance = DepositChanceFunction.Calc(score);
            if (Game.I.Random.Randf() > chance) continue;
            deps.Add(p);
        }
        return deps;
    }
    protected abstract IFunction<float, float> DepositChanceFunction { get; }
    public abstract int GetDepositScore(Cell c, Data d);

}
