
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class NaturalResource : TradeableItem
{
    protected NaturalResource(string name, Color color, int initialPrice, params ItemAttribute[] attributes) 
        : base(name, color, initialPrice, attributes)
    {
    }
    public Dictionary<MapPolygon, int> GenerateDeposits(Data data)
    {
        var polys = data.Planet.Polygons.Entities;
        var deps = new Dictionary<MapPolygon, int>();
        var scores = new Dictionary<MapPolygon, int>();
        foreach (var p in polys)
        {
            var score = scores.GetOrAdd(p, GetDepositScore);
            var chance = DepositChanceFunction.Calc(score);
            if (Game.I.Random.Randf() > chance) continue;
            var size = GenerateDepositSize(p) + (deps.ContainsKey(p) ? deps[p] : 0);
            if (size < _minDepositSize) continue;
            deps.AddOrSum(p, size);
            if (deps[p] > _overflowSize) deps[p] = _overflowSize;
            var rem = size - _overflowSize;
            if (rem <= 0) continue;

            if(_overflow == OverFlowType.Multiple) OverflowMult(p, deps, scores, rem);
            else if(_overflow == OverFlowType.Single) OverflowSingle(p, deps, rem);
        }

        return deps;
    }

    private void OverflowMult(MapPolygon p, Dictionary<MapPolygon, int> deps, 
        Dictionary<MapPolygon, int> scores, int rem)
    {
        var neighbors = p.Neighbors.Entities();
        var portions = Apportioner.ApportionLinear(rem, neighbors, n => scores.GetOrAdd(n, GetDepositScore));
        for (var i = 0; i < portions.Count; i++)
        {
            var n = neighbors.ElementAt(i);
            deps.AddOrSum(n, portions[i]);
            deps[n] = Mathf.Min(_overflowSize, deps[n]);
            if (deps[n] < _minDepositSize) deps.Remove(n);
        }
    }
    private void OverflowSingle(MapPolygon p, Dictionary<MapPolygon, int> deps, int rem)
    {
        var overflowPoly = p.Neighbors.Entities()
            .OrderBy(GetDepositScore)
            .Where(n => deps.ContainsKey(n) == false)
            .FirstOrDefault();
        if (overflowPoly != null)
        {
            deps.AddOrSum(overflowPoly, rem);
        }
    }
    protected abstract IFunction<float, float> DepositChanceFunction { get; }
    protected abstract int GetDepositScore(MapPolygon p);
    protected abstract int GenerateDepositSize(MapPolygon p);
    protected abstract int _overflowSize { get; }
    protected abstract int _minDepositSize { get; }
    protected abstract OverFlowType _overflow { get; }

    protected enum OverFlowType
    {
        None, Single, Multiple
    }
}
