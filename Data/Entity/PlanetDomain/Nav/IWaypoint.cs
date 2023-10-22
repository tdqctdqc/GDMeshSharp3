
using System.Collections.Generic;

public interface IWaypoint
{
    HashSet<int> Neighbors { get; }
    IEnumerable<MapPolygon> AssocPolys(Data data);
}