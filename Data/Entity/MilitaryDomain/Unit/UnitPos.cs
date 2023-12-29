
using System;
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
        return WaypointLoc.X != -1 && WaypointLoc.Y != -1;
    }

    public bool OnWaypoint()
    {
        return (WaypointLoc.X != -1 && WaypointLoc.Y == -1)
               || (WaypointLoc.X == -1 && WaypointLoc.Y != -1);
    }

    public bool TryGetWaypoint(Data d, out Waypoint wp)
    {
        wp = null;
        if (OnWaypoint())
        {
            var id = WaypointLoc.X != -1
                ? WaypointLoc.X
                : WaypointLoc.Y;
            wp = MilitaryDomain.GetTacWaypoint(id, d);
            return true;
        }

        return false;
    }

    public Waypoint GetWaypoint(Data d)
    {
        if (OnWaypoint() == false) throw new Exception();
        var id = WaypointLoc.X != -1
            ? WaypointLoc.X
            : WaypointLoc.Y;
        return MilitaryDomain.GetTacWaypoint(id, d);
    }

    public (Waypoint w, Waypoint v) GetAxisWps(Data d)
    {
        if (WaypointLoc.X == -1 || WaypointLoc.Y == -1) throw new Exception();

        return (MilitaryDomain.GetTacWaypoint(WaypointLoc.X, d), 
            MilitaryDomain.GetTacWaypoint(WaypointLoc.Y, d));
    }

    public void Set(Vector2 pos)
    {
        Pos = pos;
        WaypointLoc = -Vector2I.One;
    }
    public void Set(Waypoint w, Waypoint v, Vector2 pos)
    {
        Pos = pos;
        WaypointLoc = new Vector2I(w.Id, v.Id);
    }
    public void Set(Waypoint w)
    {
        Pos = w.Pos;
        WaypointLoc = new Vector2I(w.Id, -1);
    }
    public UnitPos Copy()
    {
        return new UnitPos(Pos, WaypointLoc);
    }
}