using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public Cell MouseOverCell { get; private set; }
    public Action<Cell> ChangedCell { get; set; }
    public Action<MapPolygon> ChangedPoly { get; set; }
    private TimerAction _timerAction;
    public MouseOverHandler(Data data)
    {
        _timerAction = new TimerAction(.1f, 0f,
            () =>
            {
                var mousePos = Game.I.Client.Cam().GetMousePosInMapSpace();
                Find(data, mousePos);
            });
    }
    
    public void Process(float delta)
    {
        _timerAction.Process(delta);
    }

    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {
        if (mousePosMapSpace.Y <= 0f 
            || mousePosMapSpace.Y >= data.Planet.Height)
        {
            SetPoly(null);
            SetCell(null);
            return;
        }

        Find(data, mousePosMapSpace);
    }

    
    private void Find(Data data, Vector2 mousePosMapSpace)
    {
        var c = data.Planet.PolygonAux
            .CellGrid.GetElementAtPointWhere(mousePosMapSpace, 
                c => c is RiverCell,
                data);
        if(c == null) c = data.Planet.PolygonAux
            .CellGrid.GetElementAtPoint(mousePosMapSpace, data);
        SetCell(c);

        if (c is IPolyCell single)
        {
            SetPoly(single.Polygon.Get(data));
        }
        else if (c is RiverCell r)
        {
            var edge = r.Edge.Get(data);
            var p1 = edge.HighPoly.Get(data);
            var p2 = edge.LowPoly.Get(data);
            var close =mousePosMapSpace.Offset(p1.Center, data)
                < mousePosMapSpace.Offset(p2.Center, data)
                ? p1 : p2;
            SetPoly(close);
        }
    }
    private void SetPoly(MapPolygon p)
    {
        if (p != MouseOverPoly)
        {
            MouseOverPoly = p;
            ChangedPoly?.Invoke(MouseOverPoly);
        }
    }

    private void SetCell(Cell c)
    {
        if (c != MouseOverCell)
        {
            MouseOverCell = c;
            ChangedCell?.Invoke(MouseOverCell);
        }
    }


    public void Highlight()
    {
        var client = Game.I.Client;
        var highlight = client.GetComponent<MapGraphics>().Highlighter;
        client.HighlightPoly(MouseOverPoly, 3f);
        client.HighlightCell(MouseOverCell, 5f);
    }
}
