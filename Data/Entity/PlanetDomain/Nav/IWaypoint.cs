
using System.Collections.Generic;

public interface IWaypoint : IIdentifiable
{
    HashSet<int> Neighbors { get; }
    IEnumerable<MapPolygon> AssocPolys(Data data);
}