
using Godot;

public abstract class MoveType : IModel
{
    public abstract float TerrainSpeedMod(PolyTri pt, Data d);
    public abstract bool Passable(Waypoint wp, Alliance a, bool goThruHostile, Data d);
    public abstract float PathfindCost(Waypoint wp1, Waypoint wp2,
        Alliance a, bool goThruHostile, Data d);
    public abstract float PathfindCost(Vector2 p1, PolyTri tri1, 
        Vector2 p2, PolyTri tri2, Alliance a, bool goThruHostile, Data d);
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
        Waypoint wp, bool goThruHostile, Data d)
    {
        var occR = wp.GetOccupyingRegime(d);
        if (occR == null) return true;
        if (moverAlliance.Members.Contains(occR)) return true;
        var occupier = occR.GetAlliance(d);
        if (goThruHostile && moverAlliance.AtWar.Contains(occupier)) return true;
        return false;
    }
    
    protected static bool AllianceCanPass(Alliance moverAlliance, 
        Alliance territoryAlliance, bool goThruHostile, Data d)
    {
        return moverAlliance == territoryAlliance 
               || (goThruHostile && moverAlliance.AtWar.Contains(territoryAlliance));
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
    
    protected float DefaultLandPathfindCost(
        Vector2 p1,
        PolyTri tri1, 
        Vector2 p2,
        PolyTri tri2,
        Alliance a, 
        bool goThruHostile, Data d)
    {
        var l1 = tri1.Landform(d);
        var l2 = tri2.Landform(d);
        if (l1.IsWater || l2.IsWater)
        {
            return Mathf.Inf;
        }
        
        var poly2 = d.Get<MapPolygon>(tri2.PolyId);
        var occupier = poly2.OccupierRegime.Entity(d);
        if (AllianceCanPass(a, occupier.GetAlliance(d), goThruHostile, d) == false)
        {
            return Mathf.Inf;
        }
        
        
        
        var roughnessScore = (2f + l1.MinRoughness + l2.MinRoughness) / 2f;
        var speed = BaseSpeed;
        var l = p1.GetOffsetTo(p2, d).Length();
        return l * roughnessScore / speed;
    }
}