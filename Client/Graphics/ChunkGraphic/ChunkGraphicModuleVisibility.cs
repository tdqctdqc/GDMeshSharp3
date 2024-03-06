using System;
using Godot;

public class ChunkGraphicModuleVisibility
{
    public Vector2 VisibleZoomRange { get; private set; }
    public bool VisibleByZoom { get; private set; }
    public bool VisibleOverride { get; set; }
    public Action<bool> SetZoomVisibility { get; set; }

    public ChunkGraphicModuleVisibility(Vector2 visibleZoomRange)
    {
        VisibleZoomRange = visibleZoomRange;
        VisibleByZoom = true;
        VisibleOverride = true;
    }
    
    public bool Visible()
    {
        return VisibleByZoom && VisibleOverride;
    }
    
    public void CheckVisibleTick(UiTickContext context, Data d)
    {
        var zoom = context.ZoomLevel;
        bool newVal;
        if (VisibleZoomRange.X > zoom || VisibleZoomRange.Y < zoom)
        {
            newVal = false;
        }
        else
        {
            newVal = true;
        }

        if (newVal != VisibleByZoom)
        {
            VisibleByZoom = newVal;
            SetZoomVisibility?.Invoke(VisibleByZoom);
        }
    }
}