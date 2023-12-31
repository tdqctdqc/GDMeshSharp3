
using System;
using Godot;

public class MapPos
{
    public Vector2I Pos { get; private set; }
    public Vector2I WaypointLoc { get; private set; }
    public PolyTriPosition Tri { get; private set; }
    public MapPos(Vector2I pos, Vector2I waypointLoc, PolyTriPosition tri)
    {
        Pos = pos;
        WaypointLoc = waypointLoc;
        Tri = tri;
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

    public PolyTri GetTri(Data d)
    {
        return Tri.Tri(d);
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

    public void Set(Vector2I pos, Mover.MoveData moveDat, 
        LogicWriteKey key)
    {
        var ctx = OnWaypointAxis() || OnWaypoint()
            ? MovementContext.WaypointToPoint
            : MovementContext.PointToPoint;
        Pos = pos;
        WaypointLoc = -Vector2I.One;
        SetTri(key.Data);
        key.Data.Context.AddToMovementRecord(moveDat.Id, Pos, ctx, key.Data);
    }
    public void Set(Waypoint w, Waypoint v, Mover.MoveData moveDat, 
        Vector2I pos, LogicWriteKey key)
    {
        Pos = pos;
        WaypointLoc = new Vector2I(w.Id, v.Id);
        SetTri(key.Data);
        key.Data.Context.AddToMovementRecord(moveDat.Id, Pos, MovementContext.WaypointToWaypoint, key.Data);
    }
    public void Set(Waypoint w, Mover.MoveData moveDat, 
        LogicWriteKey key)
    {
        var ctx = OnWaypointAxis() || OnWaypoint()
            ? MovementContext.WaypointToWaypoint
            : MovementContext.PointToWaypoint;

        Pos = (Vector2I)w.Pos;
        WaypointLoc = new Vector2I(w.Id, -1);
        SetTri(key.Data);
        key.Data.Context.AddToMovementRecord(moveDat.Id, Pos, ctx, key.Data);
    }

    private void SetTri(Data d)
    {
        var poly = Tri.Poly(d);
        var tri = GetTri(d);
        var rel = poly.Center.GetOffsetTo(Pos, d);
        if (tri.ContainsPoint(rel))
        {
            return;
        }

        if (poly.PointInPolyRel(rel, d))
        {
            Tri = poly.Tris.GetAtPoint(rel, d).GetPosition();
            return;
        }
        Tri = Pos.GetPolyTri(d).GetPosition();
    }
    public MapPos Copy()
    {
        return new MapPos(Pos, WaypointLoc, Tri);
    }
}