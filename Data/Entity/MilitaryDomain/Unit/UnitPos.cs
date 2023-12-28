
using Godot;

public class UnitPos
{
    public Vector2 Pos { get; private set; }
    public Vector2I WaypointLoc { get; private set; }
    public UnitPos(Vector2 pos, Vector2I waypointLoc)
    {
        Pos = pos;
        WaypointLoc = waypointLoc;
    }

    public bool OnWaypointAxis()
    {
        return WaypointLoc.X != -1 || WaypointLoc.Y != -1;
    }
}