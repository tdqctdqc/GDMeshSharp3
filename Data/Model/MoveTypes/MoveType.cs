
using Godot;

public abstract class MoveType : IModel
{
    public abstract float TerrainSpeedMod(PolyTri pt, Data d);
    public abstract bool Passable(Waypoint wp, Alliance a, bool goThruHostile, Data d);
    public abstract float PathfindCost(Waypoint wp1, Waypoint wp2, Alliance a, bool goThruHostile, Data d);
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

    protected static bool AllianceCanPass(Alliance a, 
        Waypoint wp, bool goThruHostile, Data d)
    {
        var occR = wp.GetOccupyingRegime(d);
        if (occR == null) return true;
        if (a.Members.Contains(occR)) return true;
        var occupier = occR.GetAlliance(d);
        if (goThruHostile && a.AtWar.Contains(occupier)) return true;
        return false;
    }
    
    protected float DefaultLandPathfindCost(
        Waypoint wp1,
        Waypoint wp2,
        Alliance a, 
        bool goThruHostile, Data d)
    {
        if (wp1 is ILandWaypoint l1 == false 
            || wp2 is ILandWaypoint l2 == false)
        {
            return Mathf.Inf;
        }

        if (AllianceCanPass(a, wp1, goThruHostile, d) == false 
            || AllianceCanPass(a, wp2, goThruHostile, d) == false)
        {
            return Mathf.Inf;
        }

        var roughnessScore = (2f + l1.Roughness + l2.Roughness) / 2f;
        var speed = BaseSpeed;
        var l = wp1.Pos.GetOffsetTo(wp2.Pos, d).Length();
        if (UseRoads && wp1.GetRoadWith(wp2, d) is RoadModel r)
        {
            var oldSpeed = speed;
            var oldCost = l * roughnessScore / speed;
            speed = Mathf.Max(r.SpeedMult * BaseSpeed, r.SpeedOverride);
            roughnessScore = 1f;
            var newCost = l * roughnessScore / speed;
            if (oldCost < newCost)
            {
                GD.Print($"old cost {oldCost} road cost {newCost}");
            }
        }
        return l * roughnessScore / speed;
    }
}