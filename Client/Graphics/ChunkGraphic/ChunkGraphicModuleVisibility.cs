using Godot;

public class ChunkGraphicModuleVisibility
{
    public Vector2 VisibleZoomRange { get; private set; }
    public bool VisibleByZoom { get; private set; }
    public bool VisibleOverride { get; set; }

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
        if (VisibleZoomRange.X > zoom || VisibleZoomRange.Y < zoom)
        {
            VisibleByZoom = false;
        }
        else
        {
            VisibleByZoom = true;
        }
    }
}