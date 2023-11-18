using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class NavWaypoints : Entity
{
    public Waypoint Get(int id) => Waypoints[id];
    public Dictionary<int, Waypoint> Waypoints { get; private set; }
    public Dictionary<Vector2, Waypoint> WaypointsByPos { get; private set; }
    public Dictionary<int, int> PolyCenterIds { get; private set; }
    public Dictionary<Vector2, List<int>> PolyNavPaths { get; private set; }
    public static NavWaypoints Create(GenWriteKey key)
    {
        var n = new NavWaypoints(key.Data.IdDispenser.TakeId(), new Dictionary<int, Waypoint>(), new Dictionary<int, int>(),
            new Dictionary<Vector2, List<int>>(), new Dictionary<Vector2, Waypoint>());
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private NavWaypoints(int id, 
        Dictionary<int, Waypoint> waypoints,
        Dictionary<int, int> polyCenterIds, 
        Dictionary<Vector2, List<int>> polyNavPaths,
        Dictionary<Vector2, Waypoint> waypointsByPos) : base(id)
    {
        Waypoints = waypoints;
        PolyCenterIds = polyCenterIds;
        PolyNavPaths = polyNavPaths;
        WaypointsByPos = waypointsByPos;
    }

    public void AddWaypoint(Waypoint wp, GenWriteKey key)
    {
        Waypoints.Add(wp.Id, wp); 
        WaypointsByPos.Add(wp.Pos, wp);
    }
    public void MakeCenterPoint(MapPolygon poly, Waypoint wp, GenWriteKey key)
    {
        PolyCenterIds.Add(poly.Id, wp.Id);
    }

    public Waypoint GetPolyCenterWaypoint(MapPolygon poly)
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

    public IEnumerable<Waypoint> GetPolyAssocWaypoints(MapPolygon poly, Data data)
    {
        var center = GetPolyCenterWaypoint(poly);
        var assoc = new HashSet<Waypoint>{center};
        var frontier = new Queue<Waypoint>();
        frontier.Enqueue(center);
        while (frontier.Count() > 0)
        {
            var current = frontier.Dequeue();
            if (current.AssociatedWithPoly(poly))
            {
                foreach (var n in current.Neighbors)
                {
                    var nWp = data.Planet.NavWaypoints.Waypoints[n];
                    if (assoc.Contains(nWp) == false && nWp.AssociatedWithPoly(poly))
                    {
                        frontier.Enqueue(nWp);
                        assoc.Add(nWp);
                    }
                }
            }
        }

        return assoc;
    }
}
