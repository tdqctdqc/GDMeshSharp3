
using System.Collections.Generic;
using Godot;

public interface IWaypoint : IIdentifiable, Mover.IPathfindNode
{
    HashSet<int> Neighbors { get; }
    IEnumerable<MapPolygon> AssocPolys(Data data);
    Vector2 Pos { get; }
}