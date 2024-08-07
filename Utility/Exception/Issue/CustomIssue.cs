
using System;
using Godot;

public class CustomIssue : Issue
{
    private Action<MeshBuilder> _draw;
    public CustomIssue(Vector2 pos, string message,
        Action<MeshBuilder> draw) 
        : base(pos, message)
    {
        _draw = draw;
    }

    public override void Draw(Client c)
    {
        var overlay = c.GetComponent<MapGraphics>()
            .DebugOverlay;
        overlay.Clear();
        overlay.Draw(_draw, Pos);
    }
}