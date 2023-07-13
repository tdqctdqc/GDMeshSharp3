using Godot;
using System;
using System.Collections.Generic;

public partial class WorldCameraController : Camera2D, ICameraController
{
    public float ScaledZoomOut => _zoomLevel / _maxZoomLevel;
    public float SmoothedZoomOut => ShapingFunctions.EaseInCubic(_zoomLevel, _maxZoom, _minZoom);
    public float ZoomOut => Zoom.X;
    public float MaxZoomOut => _maxZoom;

    private float _udScrollSpeed = 500f;
    private float _lrScrollSpeed = .05f;
    private float _zoomIncr = .01f;
    private float _zoomLevel = .9f;
    private float _maxZoom = 50f;
    private float _minZoom = .5f;
    private float _minZoomLevel = .05f;
    private float _maxZoomLevel = .9f;
    public float XScrollRatio { get; private set; }
    private Data _data;

    public static WorldCameraController Construct(Data data)
    {
        var c = new WorldCameraController();
        c.Setup(data);
        return c;
    }
    private WorldCameraController()
    {
        Zoom = Vector2.One / 100f;

        UpdateZoom();
    }
    public void Setup(Data data)
    {
        
        _data = data;
    }


    private Vector2 GetOffset(Vector2 mapPos)
    {
        var off1 = mapPos - Position;
        var off2 = (off1 + Vector2.Right * _data.Planet.Width);
        var off3 = (off1 + Vector2.Left * _data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }

    public Vector2 GetMousePosInMapSpace()
    {
        if(_data.Planet == null) return Vector2.Inf;
        if(_data.Planet.Info == null) return Vector2.Inf;
        
        var mapWidth = _data.Planet.Width;
        var scrollDist = mapWidth * XScrollRatio;
        
        var mousePosGlobal = GetGlobalMousePosition();
        var mapSpaceMousePos =  new Vector2(mousePosGlobal.X + scrollDist, mousePosGlobal.Y);
        while (mapSpaceMousePos.X > mapWidth) mapSpaceMousePos += Vector2.Left * mapWidth;
        while (mapSpaceMousePos.X < 0f) mapSpaceMousePos += Vector2.Right * mapWidth;
        return mapSpaceMousePos;
    }

    public Vector2 GetMapPosInGlobalSpace(Vector2 mapPos)
    {
        var mapWidth = _data.Planet.Width;   
        var scrollDist = mapWidth * XScrollRatio;
        
        var globalSpace = new Vector2(mapPos.X - scrollDist, mapPos.Y);
        
        while (globalSpace.X > mapWidth / 2f) globalSpace += Vector2.Left * mapWidth;
        while (globalSpace.X < -mapWidth / 2f) globalSpace += Vector2.Right * mapWidth;
        
        return globalSpace;
    }

    public void Process(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            HandleMouseButton(mb);
        }
    }
    public override void _Process(double deltaD)
    {
        var delta = (float) deltaD;
        var mult = 1f;
        if (Input.IsKeyPressed( Key.Shift)) mult = 3f;
        if(Input.IsKeyPressed(Key.W))
        {
            Position += Vector2.Up * delta / Zoom * _udScrollSpeed * mult;
        }
        if(Input.IsKeyPressed(Key.S))
        {
            Position += Vector2.Down * delta / Zoom * _udScrollSpeed * mult;
        }
        if(Input.IsKeyPressed(Key.A))
        {
            XScrollRatio -= delta / Zoom.Length() * _lrScrollSpeed * mult;
            if (XScrollRatio > 1f) XScrollRatio -= 1f;
            if (XScrollRatio < 0f) XScrollRatio += 1f;
        }
        if(Input.IsKeyPressed(Key.D))
        {
            XScrollRatio += delta / Zoom.Length() * _lrScrollSpeed * mult;
            if (XScrollRatio > 1f) XScrollRatio -= 1f;
            if (XScrollRatio < 0f) XScrollRatio += 1f;
        }
        if(Input.IsKeyPressed(Key.Q))
        {
            Position += Vector2.Left * delta / Zoom * _udScrollSpeed * mult;
        }
        if(Input.IsKeyPressed(Key.E))
        {
            Position += Vector2.Right * delta / Zoom * _udScrollSpeed * mult;
        }
    }

    
    private void HandleMouseButton(InputEventMouseButton mb)
    {
        if(mb.ButtonIndex == MouseButton.WheelDown)
        {
            _zoomLevel += _zoomIncr;
        }
        if(mb.ButtonIndex == MouseButton.WheelUp)
        {
            _zoomLevel -= _zoomIncr;
        }

        UpdateZoom();
    }
    
    private void UpdateZoom()
    {
        _zoomLevel = Mathf.Clamp(_zoomLevel, _minZoomLevel, _maxZoomLevel);
        var zoomFactor = ShapingFunctions.EaseInCubic(_zoomLevel, _maxZoom, _minZoom);
        zoomFactor = Mathf.Clamp(zoomFactor, _minZoom, _maxZoom);
        Zoom = Vector2.One / zoomFactor;
    }
}