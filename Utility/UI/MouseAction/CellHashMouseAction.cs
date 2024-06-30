
using System;
using System.Collections.Generic;
using Godot;

public class CellHashMouseAction : MouseHoldAction
{
    private Func<(Cell prospect, HashSet<Cell> already, MouseAuxButton aux), bool> _valid;
    private HashSet<Cell> _cells;
    private Data _data;
    private MouseOverHandler _mouseOverHandler;
    private MouseAuxButton _aux;
    private Action<HashSet<Cell>> _defaultReleaseAction;
    private Action<HashSet<Cell>> _ctrlReleaseAction;
    private Action<HashSet<Cell>> _shiftReleaseAction;

    public enum MouseAuxButton
    {
        Ctrl,
        Shift, 
        Default
    }

    // public Action<HashSet<Cell>> MouseReleased { get; set; }
    
    public CellHashMouseAction(MouseOverHandler mouseOverHandler,
        Func<(Cell prospect, HashSet<Cell> already, MouseAuxButton aux), bool> valid,
        MouseButtonMask button, Data data) : base(button)
    {
        _data = data;
        _valid = valid;
        _mouseOverHandler = mouseOverHandler;
    }

    protected override void MouseDown(InputEventMouse m)
    {
        if (m.CtrlPressed)
        {
            _aux = MouseAuxButton.Ctrl;
        }
        else if (m.ShiftPressed)
        {
            _aux = MouseAuxButton.Shift;
        }
        else
        {
            _aux = MouseAuxButton.Default;
        }
        _cells = new HashSet<Cell>();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell is null
            || _valid((cell, _cells, _aux)) == false)
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
            || _valid((cell, _cells, _aux)) == false) return;
        _cells.Add(cell);
    }

    protected override void MouseUp(InputEventMouse m)
    {
        if (_cells.Count == 0) return;
        if (_aux == MouseAuxButton.Ctrl)
        {
            _ctrlReleaseAction?.Invoke(_cells);
        }
        else if (_aux == MouseAuxButton.Shift)
        {
            _shiftReleaseAction?.Invoke(_cells);
        }
        else if(_aux == MouseAuxButton.Default)
        {
            _defaultReleaseAction?.Invoke(_cells);
        }
        _cells = null;
    }


    public void AddDefaultAction(Action<HashSet<Cell>> action)
    {
        _defaultReleaseAction += action;
    }
    public void AddCtrlAction(Action<HashSet<Cell>> action)
    {
        _ctrlReleaseAction += action;
    }
    public void AddShiftAction(Action<HashSet<Cell>> action)
    {
        _shiftReleaseAction += action;
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
                mb.DrawPolygon(cell.RelBoundary, Colors.Yellow.Tint(.5f));
            }, cell.RelTo);
        }
    }
}