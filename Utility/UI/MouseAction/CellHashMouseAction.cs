
using System;
using System.Collections.Generic;
using Godot;

public class CellHashMouseAction : MouseAction
{
    private Func<(Cell prospect, HashSet<Cell> already), bool> _valid;
    protected HashSet<Cell> _cells;
    private Data _data;
    private MouseOverHandler _mouseOverHandler;
    

    // public Action<HashSet<Cell>> MouseReleased { get; set; }
    
    public CellHashMouseAction(MouseOverHandler mouseOverHandler,
        Func<(Cell prospect, HashSet<Cell> already), bool> valid,
        MouseButtonMask button, Data data) : base(button)
    {
        _data = data;
        _valid = valid;
        _mouseOverHandler = mouseOverHandler;
    }

    protected override void MouseDown(InputEventMouse m)
    {
        _cells = new HashSet<Cell>();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null
            || _valid((cell, _cells)) == false)
        {
            return;
        }

        _cells.Add(cell);
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        if (_cells.Count == 0) return;
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null
            || _valid((cell, _cells)) == false) return;
        _cells.Add(cell);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        if (_cells.Count == 0) return;
        _cells = null;
    }
    
    public override void Highlight(Client c, MapOverlayDrawer overlay)
    {
        if (_cells is null || _cells.Count == 0) return;
        foreach (var cell in _cells)
        {
            overlay.Draw(mb =>
            {
                mb.DrawPolygon(cell.RelBoundary, Colors.Yellow.Tint(.5f));
            }, cell.RelTo);
        }
    }
}