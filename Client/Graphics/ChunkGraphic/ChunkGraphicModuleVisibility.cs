using System;
using Godot;

public class ChunkGraphicModuleVisibility
{
    public Vector2 VisibleZoomRange { get; private set; }
    public bool VisibleByZoom { get; private set; }
    public bool VisibleOverride { get; set; }
    public Action<bool> SetVisibility { get; set; }

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
        var oldVisibility = Visible();
        var zoom = context.ZoomLevel;
        bool newZoomVisibility;
        if (VisibleZoomRange.X > zoom || VisibleZoomRange.Y < zoom)
        {
            newZoomVisibility = false;
        }
        else
        {
            newZoomVisibility = true;
        }

        if (newZoomVisibility != VisibleByZoom)
        {
            VisibleByZoom = newZoomVisibility;
        }

        var newVisibility = Visible();
        if (newVisibility != oldVisibility)
        {
            SetVisibility?.Invoke(newVisibility);
        }
    }
}