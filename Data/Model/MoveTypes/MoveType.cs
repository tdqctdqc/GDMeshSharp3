
using Godot;

public abstract class MoveType : IModel
{
    public abstract float TerrainSpeedMod(PolyTri pt, Data d);
    public abstract bool Passable(Waypoint wp, Alliance a, Data d);
    public abstract float PathfindCost(Waypoint wp, Alliance a, Data d);
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

    protected static bool CanPassByAlliance(Alliance a, Waypoint wp, Data d)
    {
        var occR = wp.GetOccupyingRegime(d);
        if (occR == null) return true;
        var occupier = occR.GetAlliance(d);
        if (occupier == a) return true;
        if (a.AtWar.Contains(occupier)) return true;
        return false;
    }
}