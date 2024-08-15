using System;
using System.Collections.Generic;
using Godot;

public abstract class MoveType : IModel
{
    protected abstract float TerrainCostInstantaneous(Cell pt, Data d);

    public bool PassableFriendly(Cell cell, Regime r, Data d)
    {
        return TerrainPassable(cell, d) && 
            IsFriendly(r, cell, d);
    }
    public bool PassableFriendlyOrRival(Cell cell, 
        Regime r, Data d)
    {
        return TerrainPassable(cell, d) && 
               IsFriendlyOrRival(r, cell, d);
    }
    public abstract bool TerrainPassable(Cell p, Data d);

    public float PathCost(List<Cell> path, Data d)
    {
        var cost = 0f;
        for (var i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];
            cost += EdgeCost(from, to, d);
        }
        return cost;
    }
    public float EdgeCost(Cell from, Cell to, Data d)
    {
        var l = from.GetCenter().Offset(to.GetCenter(), d).Length();
        var terrCostPerLength = TerrainCostPerLength(from, to, d);
        if (UseRoads)
        {
            var r = from.GetRoadWith(to, d);
            if (r != null)
            {
                var roadCostPerLength = RoadCostPerLength(r);
                if (roadCostPerLength < terrCostPerLength)
                {
                    return l * roadCostPerLength;
                }
            }
        }
        return l * terrCostPerLength;
    }

    public float RoadCostPerLength(RoadModel r)
    {
        var cost = r.CostOverride;
        if (r.UseSpeedOverride)
        {
            var speedRatio = BaseSpeed / r.SpeedOverride;
            return cost * speedRatio;
        }
        return cost;
    }
    public float TerrainCostPerLength(Cell cell1, Cell cell2, Data d)
    {
        return (TerrainCostInstantaneous(cell1, d) 
                + TerrainCostInstantaneous(cell2, d)) / 2f;
    }
    public bool UseRoads { get; private set; }
    public float BaseSpeed { get; private set; }
    public int Id { get; private set; }
    public string Name { get; private set; }
    protected MoveType()
    {
    }
    protected static bool IsFriendly(Regime moverRegime, 
        Cell cell, Data d)
    {
        if (cell is LandCell l == false) return true;
        return moverRegime.Id == cell.Controller.RefId;
    }
    protected static bool IsFriendlyOrRival(Regime moverRegime, 
        Cell cell, Data d)
    {
        if (cell is LandCell l == false) return false;
        var controllerAlliance = cell.Controller.Get(d);
        return moverRegime == controllerAlliance
            || moverRegime.IsRivals(controllerAlliance, d);
    }
}