
using System.Collections.Generic;
using Godot;

public class NullWaypoint : Waypoint
{
    public NullWaypoint(GenWriteKey key, int id, PolyTriPosition tri, Vector2 pos, MapPolygon poly1, MapPolygon poly2 = null, MapPolygon poly3 = null, MapPolygon poly4 = null) : base(key, id, tri, pos, poly1, poly2, poly3, poly4)
    {
    }

    public NullWaypoint(int id, HashSet<int> neighbors, Vector4I associatedPolyIds, Vector2 pos, EntityRef<Alliance> controller) : base(id, neighbors, associatedPolyIds, pos, controller)
    {
    }
}