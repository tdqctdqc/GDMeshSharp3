
using System;
using Godot;

public class CustomIssue : Issue
{
    private Action<MeshBuilder> _draw;
    public CustomIssue(Vector2 pos, string message, int tick,
        params (string, Action<MeshBuilder>)[] layers) 
        : base(pos, message, tick)
    {
        for (var i = 0; i < layers.Length; i++)
        {
            var (name, action) = layers[i];
            AddLayer(name, action);
        }
    }
}