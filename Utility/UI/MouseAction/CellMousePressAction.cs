
using System;
using Godot;

public class CellMousePressAction : MousePressAction
{
    private MouseOverHandler _mouseOverHandler;
    private Func<Cell, bool> _valid;
    public Action<Cell> MouseReleased { get; set; }
    public CellMousePressAction(MouseButtonMask button,
        MouseOverHandler mouseOverHandler,
        Func<Cell, bool> valid)
        : base(button)
    {
        _valid = valid;
        _mouseOverHandler = mouseOverHandler;
    }

    protected override void MouseUp(InputEventMouse m)
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null) return;
        MouseReleased?.Invoke(cell);
    }

    public override void Highlight(Client c, MapOverlayDrawer overlay)
    {
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null) return;
        overlay.Draw(mb =>
        {
            mb.DrawPolygon(cell.RelBoundary, Colors.Yellow.Tint(.5f));
        },  cell.RelTo);
    }
}