using System;
using Godot;

public abstract class MoveType : IModel
{
    protected abstract float TerrainCostInstantaneous(PolyCell pt, Data d);

    public bool Passable(PolyCell cell, Alliance a, Data d)
    {
        return TerrainPassable(cell, d) && 
            AllianceCanPass(a, cell, d);
    }
    public abstract bool TerrainPassable(PolyCell p, Data d);

    public float EdgeCost(PolyCell from, PolyCell to, Data d)
    {
        var l = from.GetCenter().GetOffsetTo(to.GetCenter(), d).Length();
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
    public float TerrainCostPerLength(PolyCell cell1, PolyCell cell2, Data d)
    {
        return (TerrainCostInstantaneous(cell1, d) 
                + TerrainCostInstantaneous(cell2, d)) / 2f;
    }
    public bool UseRoads { get; private set; }
    public float BaseSpeed { get; private set; }
    public int Id { get; private set; }
    public string Name { get; private set; }
    protected MoveType(bool useRoads, float baseSpeed, string name)
    {
        UseRoads = useRoads;
        BaseSpeed = baseSpeed;
        Name = name;
    }
    protected static bool AllianceCanPass(Alliance moverAlliance, 
        PolyCell cell, Data d)
    {
        if (cell is LandCell l == false) return true;
        var controllerAlliance = cell.Controller.Entity(d).GetAlliance(d);
        return moverAlliance == controllerAlliance;
    }
}