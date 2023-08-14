using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class Nav : Entity
{
    public Dictionary<int, Waypoint> Waypoints { get; private set; }
    public Dictionary<int, int> PolyCenterIds { get; private set; }
    public Dictionary<Vector2, List<int>> PolyNavPaths { get; private set; }
    public static Nav Create(GenWriteKey key)
    {
        var n = new Nav(-1, new Dictionary<int, Waypoint>(), new Dictionary<int, int>(),
            new Dictionary<Vector2, List<int>>());
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private Nav(int id, Dictionary<int, Waypoint> waypoints,
        Dictionary<int, int> polyCenterIds, Dictionary<Vector2, List<int>> polyNavPaths) : base(id)
    {
        Waypoints = waypoints;
        PolyCenterIds = polyCenterIds;
        PolyNavPaths = polyNavPaths;
    }

    public void MakeCenterPoint(MapPolygon poly, Waypoint p, GenWriteKey key)
    {
        PolyCenterIds.Add(poly.Id, p.Id);
    }

    public Waypoint GetPolyWaypoint(MapPolygon poly)
    {
        return Waypoints[PolyCenterIds[poly.Id]];
    }

    public IEnumerable<Waypoint> GetPolyPath(MapPolygon p1, MapPolygon p2)
    {
        var hi = p1.Id > p2.Id ? p1.Id : p2.Id;
        var lo = p1.Id < p2.Id ? p1.Id : p2.Id;

        var k = new Vector2(hi, lo);
        if (PolyNavPaths.ContainsKey(k) == false) return null;
        var p = PolyNavPaths[k].Select(i => Waypoints[i]);
        if (hi == p2.Id) return p.Reverse();
        return p;
    }
}
