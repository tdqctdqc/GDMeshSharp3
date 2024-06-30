
using System;
using System.Collections.Generic;
using Godot;

public class CellLineMouseAction : MouseAction
{
    private Func<Cell, bool> _valid;
    private List<Cell> _cells;
    private Data _data;
    private MouseOverHandler _mouseOverHandler;
    public Action<List<Cell>> MouseReleased { get; set; }

    public CellLineMouseAction(MouseButtonMask button,
        Func<Cell, bool> valid,
        Data data,
        MouseOverHandler mouseOverHandler) 
        : base(button)
    {
        _data = data;
        _valid = valid;
        _mouseOverHandler = mouseOverHandler;
    }

    protected override void MouseDown(InputEventMouse m)
    {
        _cells = new List<Cell>();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null || _valid(cell) == false) return;
        _cells.Add(cell);
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        if (_cells.Count == 0) return;
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null || _valid(cell) == false) return;
        var last = _cells[^1];
        if (cell == last) return;
        if (cell.Neighbors.Contains(last.Id) == false) return;
        var index = _cells.IndexOf(cell);
        if (index != -1)
        {
            _cells = _cells.GetRange(0, index + 1);
            return;
        }
        _cells.Add(cell);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        if (_cells.Count == 0) return;
        MouseReleased?.Invoke(_cells);
        _cells = null;
    }

    public override void Highlight(Client c)
    {
        if (_cells is null || _cells.Count == 0) return;
        var highlighter = c.GetComponent<MapGraphics>()
            .Highlighter;
        foreach (var cell in _cells)
        {
            highlighter.Draw(mb =>
            {
                mb.DrawPolygon(cell.RelBoundary, Colors.Blue.Tint(.5f));
            }, cell.RelTo);
        }
        highlighter.Draw(mb =>
        {
            mb.DrawCellPath(Vector2.Zero, _cells,
                Colors.Red, 5f, _data);
        }, Vector2.Zero);
        
    }
}