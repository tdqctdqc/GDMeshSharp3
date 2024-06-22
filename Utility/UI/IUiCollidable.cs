using System.Collections.Generic;
using Godot;

public interface IUiCollidable
{
    Vector2 RelTo { get; }
    int Z { get; }
    IEnumerable<Vector2[]> RelPolygonBoundaries { get; }
    void Handle(InputEvent e, Vector2 pos, Client c);
    bool IsCapturing();
    bool Captures(InputEvent e);
}