using System.Collections.Generic;
using Godot;

public interface IMapPathfindNode
{
    Vector2 Pos { get; }
    PolyTriPosition Tri { get; }
    IEnumerable<IMapPathfindNode> Neighbors(Data d);
}