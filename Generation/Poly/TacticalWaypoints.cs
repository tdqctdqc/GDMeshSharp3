
using System.Collections.Generic;
using Godot;
using MessagePack;

public class TacticalWaypoints : Entity
{
    public Dictionary<int, Waypoint> Waypoints { get; private set; }
    public Dictionary<int, HashSet<int>> PolyAssocWaypoints { get; private set; }
    public Dictionary<Vector2, int> ByPos { get; private set; }
    public static TacticalWaypoints Create(GenWriteKey key,
        Dictionary<int, Waypoint> wps)
    {
        var polyAssocWaypoints = new Dictionary<int, HashSet<int>>();
        var byPos = new Dictionary<Vector2, int>();
        foreach (var wp in wps.Values)
        {
            byPos.Add(wp.Pos, wp.Id);
            foreach (var poly in wp.AssocPolys(key.Data))
            {
                var set = polyAssocWaypoints.GetOrAdd(poly.Id, p => new HashSet<int>());
                set.Add(wp.Id);
            }
        }
        var n = new TacticalWaypoints(key.Data.IdDispenser.TakeId(), 
            wps,
            polyAssocWaypoints,
            byPos);
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private TacticalWaypoints(int id,
        Dictionary<int, Waypoint> waypoints,
        Dictionary<int, HashSet<int>> polyAssocWaypoints,
        Dictionary<Vector2, int> byPos) : base(id)
    {
        Waypoints = waypoints;
        PolyAssocWaypoints = polyAssocWaypoints;
        ByPos = byPos;
    }
}