using System;
using Godot;

public abstract class MoveType : IModel
{
    public abstract float TerrainCostInstantaneous(PolyTri pt, Data d);

    public bool Passable(Waypoint wp, Alliance a, Data d)
    {
        return TerrainPassable(wp.Tri.Tri(d), d) && 
            AllianceCanPass(a,
            wp.GetOccupyingRegime(d).GetAlliance(d), d);
    }
    public abstract bool TerrainPassable(PolyTri p, Data d);

    public float StratMoveEdgeCost(Waypoint from, Waypoint to, Data d)
    {
        var l = from.Pos.GetOffsetTo(to.Pos, d).Length();
        var terrCostPerLength = TerrainCostPerLength(from.Tri.Tri(d), to.Tri.Tri(d), d);
        if (UseRoads)
        {
            var r = from.GetRoadWith(to, d);
            if (r != null)
            {
                var roadCostPerLength = RoadCostPerLength(r);
                if (roadCostPerLength < terrCostPerLength) return l * roadCostPerLength;
            }
        }
        return l * terrCostPerLength;
    }

    public float RoadCostPerLength(RoadModel r)
    {
        var cost = r.CostOverride;
        if (r.UseSpeedOverride)
        {
            var costRatio = cost / r.SpeedOverride;
            return BaseSpeed * costRatio;
        }
        return cost;
    }
    public float TerrainCostPerLength(PolyTri tri1, PolyTri tri2, Data d)
    {
        return (TerrainCostInstantaneous(tri1, d) 
                + TerrainCostInstantaneous(tri2, d)) / 2f;
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
        Alliance territoryAlliance, Data d)
    {
        return moverAlliance == territoryAlliance;
    }
}