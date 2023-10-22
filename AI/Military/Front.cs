
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Front
{
    public HashSet<Waypoint> Frontline { get; private set; }

    public static Front Construct()
    {
        return new Front(new HashSet<Waypoint>());
    }

    [SerializationConstructor]
    private Front(HashSet<Waypoint> frontline)
    {
        Frontline = frontline;
    }
    public bool Trim(HashSet<Waypoint> controlled, ProcedureWriteKey key)
    {
        foreach (var wp in Frontline.ToList())
        {
            if (controlled.Contains(wp) == false)
            {
                Frontline.Remove(wp);
            }
        }

        if (Frontline.Count() == 0) return false;
        var floodFill = FloodFill<Waypoint>
            .GetFloodFill(Frontline.First(),
                controlled.Contains,
                w => w.GetNeighboringWaypoints(key.Data));
        if (floodFill.Count() != Frontline.Count()) return false;
        return true;
    }
}