using Godot;

public interface IMouseAction
{
    void Process(InputEventMouse m);
    void Highlight(Client c, MapOverlayDrawer overlay);
}