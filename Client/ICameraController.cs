using Godot;

public interface ICameraController : IClientComponent
{
    Vector2 Position { get; }
    Vector2 GetMousePosInMapSpace();
    Vector2 GetMapPosInGlobalSpace(Vector2 mapPos);
    Vector2 GetGlobalMousePosition();
    float XScrollRatio { get; }
    void Process(InputEvent e);
    float ScaledZoomOut { get; }
    float MaxZoomOut { get; }
    float ZoomOut { get; }

}
