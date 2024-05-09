
using System.Collections.Generic;
using Godot;

public class DrawCellEdgeLine : MouseHoldAction
{
    private List<Vector2I> _edges;
    private Data _data;
    private MouseOverHandler _mouseOverHandler;
    public DrawCellEdgeLine(MouseButtonMask button,
        Data data,
        MouseOverHandler mouseOverHandler) : base(button)
    {
        _data = data;
        _mouseOverHandler = mouseOverHandler;
    }

    protected override void MouseDown(InputEventMouse m)
    {
        _edges = new List<Vector2I>();
        var edge = _mouseOverHandler.MouseOverCell.GetIdEdgeKey(
            _mouseOverHandler.SecondClosest);
        _edges.Add(edge);
    }

    protected override void MouseHeld(InputEventMouse m)
    {
        var edge = _mouseOverHandler.MouseOverCell.GetIdEdgeKey(
            _mouseOverHandler.SecondClosest);
        var last = _edges[^1];
        if (edge == last) return;
        if (incident(edge, last) == false)
        {
            return;
        }

        var index = _edges.IndexOf(edge);
        if (index != -1)
        {
            _edges = _edges.GetRange(0, index + 1);
            return;
        }

        if (_edges.Count > 1)
        {
            if (triple(edge, _edges[^1], _edges[^2]))
            {
                _edges[_edges.Count - 1] = edge;
                return;
            }
        }
        
        _edges.Add(edge);

        bool incident(Vector2I e1, Vector2I e2)
        {
            var a = e1.X;
            var b = e1.Y;
            var c = -1;
            if (e2.X != a && e2.X != b)
            {
                c = e2.X;
            }
            if (e2.Y != a && e2.Y != b)
            {
                if (c != -1) return false;
                c = e2.Y;
            }

            var c1 = PlanetDomainExt.GetPolyCell(a, _data);
            var c2 = PlanetDomainExt.GetPolyCell(b, _data);
            var c3 = PlanetDomainExt.GetPolyCell(c, _data);

            return c1.Neighbors.Contains(c2.Id)
                && c2.Neighbors.Contains(c3.Id)
                && c3.Neighbors.Contains(c1.Id);
        }

        bool triple(Vector2I e1, Vector2I e2, Vector2I e3)
        {
            var a = e1.X;
            var b = e1.Y;
            var c = -1;

            return check(e3) && check(e2);
            bool check(Vector2I e)
            {
                if (e.X != a && e.X != b
                             && e.X != c)
                {
                    if (c != -1) return false;
                    c = e.X;
                }
                if (e.Y != a && e.Y != b
                             && e.Y != c)
                {
                    if (c != -1) return false;
                    c = e.Y;
                }

                return true;
            }
        }
    }

    protected override void MouseUp(InputEventMouse m)
    {
        _edges.Clear();
    }

    public override void Highlight(Client c)
    {
        if (_edges == null) return;
        var highlighter = c.GetComponent<MapGraphics>()
            .Highlighter;
        for (var i = 0; i < _edges.Count; i++)
        {
            var edge = _edges[i];
            var c1 = PlanetDomainExt.GetPolyCell(edge.X, c.Data);
            var c2 = PlanetDomainExt.GetPolyCell(edge.Y, c.Data);
            highlighter.Draw(mb => 
                mb.DrawPolyCellEdge(c1, c2, c => Colors.Red, 
                    3f, Vector2.Zero, c.Data), Vector2.Zero);
        }
    }
}