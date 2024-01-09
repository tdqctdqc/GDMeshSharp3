
using System;
using Godot;

public class MapPos
{
    public Vector2 Pos { get; private set; }
    public PolyTriPosition Tri { get; private set; }
    public MapPos(Vector2 pos, PolyTriPosition tri)
    {
        Pos = pos;
        Tri = tri;
    }

    public PolyTri GetTri(Data d)
    {
        return Tri.Tri(d);
    }
    public void Set(Vector2 pos, MoveData moveDat, 
        LogicWriteKey key)
    {
        var ctx = MovementContext.PointToPoint;
        Pos = pos;
        SetTri(key.Data);
        key.Data.Context.AddToMovementRecord(moveDat.Id, Pos, ctx, key.Data);
    }
    
    public void Set(Waypoint w, MoveData moveDat, 
        LogicWriteKey key)
    {
        var ctx = MovementContext.PointToPoint;
        Pos = (Vector2I)w.Pos;
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
        return new MapPos(Pos, Tri);
    }
}