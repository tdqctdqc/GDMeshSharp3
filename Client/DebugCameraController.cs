using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DebugCameraController : Camera2D
{
    private Node2D _controlled;
    private float _zoomLevel;
    private float _maxZoom = 10f, _minZoom = .1f;
    private float _zoomIncr = .1f;
    private float _scrollSpeed = 500f;
    public DebugCameraController(Node2D controlled)
    {
        _controlled = controlled;
        _zoomLevel = Zoom.X;
    }

    public override void _Process(double deltaD)
    {
        var delta = (float) deltaD;
        var mult = 1f;
        if (Input.IsKeyPressed( Godot.Key.Shift)) mult = 3f;
        if(Input.IsKeyPressed(Godot.Key.W))
        {
            _controlled.Position -= Vector2.Up * delta * Zoom * _scrollSpeed * mult;
        }
        if(Input.IsKeyPressed(Godot.Key.S))
        {
            _controlled.Position -= Vector2.Down * delta * Zoom * _scrollSpeed * mult;
        }
        if(Input.IsKeyPressed(Godot.Key.A))
        {
            _controlled.Position -= Vector2.Left * delta * Zoom * _scrollSpeed * mult;
        }
        if(Input.IsKeyPressed(Godot.Key.D))
        {
            _controlled.Position -= Vector2.Right * delta * Zoom * _scrollSpeed * mult;
        }
        
        if(Input.IsKeyPressed(Godot.Key.Z))
        {
            _zoomLevel -= _zoomIncr;
            UpdateZoom();

        }
        if(Input.IsKeyPressed(Godot.Key.X))
        {
            _zoomLevel += _zoomIncr;
            UpdateZoom();
        }

    }
    
    private void UpdateZoom()
    {
        _controlled.Scale = Vector2.One * _zoomLevel;
    }
}
