using System.Collections.Generic;
using Godot;

public interface IUiCollidable
{
    Vector2 RelTo { get; }
    int ZIndex { get; }
    IEnumerable<Vector2[]> RelPolygonBoundaries { get; }
    void Handle(InputEvent e, Vector2 pos, Client c);
    bool Active(InputEvent e);
}