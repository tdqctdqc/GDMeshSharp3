using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Waypoint : IIdentifiable
{
    public int Id { get; private set; }
    public HashSet<int> Neighbors { get; private set; }
    public Vector2 Pos { get; private set; }
    public Vector4I AssociatedPolyIds { get; private set; }
    public PolymorphMember<WaypointData> WaypointData { get; private set; }

    public static Waypoint Construct(GenWriteKey key, int id, Vector2 pos, MapPolygon poly1,
        MapPolygon poly2 = null, MapPolygon poly3 = null, MapPolygon poly4 = null)
    {
        var associatedPolyIds = new Vector4I(poly1.Id, 
            poly2 != null ? poly2.Id : -1, 
            poly3 != null ? poly3.Id : -1,            
            poly4 != null ? poly4.Id : -1
        );
        return new Waypoint(id, poly1.GetChunk(key.Data).Coords, new HashSet<int>(), associatedPolyIds,
            pos, PolymorphMember<WaypointData>.Construct(null));
    }
    [SerializationConstructor] private Waypoint(int id, Vector2 chunkCoords, 
        HashSet<int> neighbors, Vector4I associatedPolyIds, Vector2 pos, 
        PolymorphMember<WaypointData> waypointData)
    {
        AssociatedPolyIds = associatedPolyIds;
        Id = id;
        Neighbors = neighbors;
        Pos = pos;
        WaypointData = waypointData;
    }

    public void SetType(WaypointData t, GenWriteKey key)
    {
        WaypointData = PolymorphMember<WaypointData>.Construct(t);
    }

    public bool AssociatedWithPoly(MapPolygon poly)
    {
        return AssociatedPolyIds.X == poly.Id
               || AssociatedPolyIds.Y == poly.Id
               || AssociatedPolyIds.Z == poly.Id
               || AssociatedPolyIds.W == poly.Id;
    }

    public int NumAssocPolys()
    {
        if (AssociatedPolyIds.X == -1) return 0;
        if (AssociatedPolyIds.Y == -1) return 1;
        if (AssociatedPolyIds.Z == -1) return 2;
        if (AssociatedPolyIds.W == -1) return 3;
        return 4;
    }
}
