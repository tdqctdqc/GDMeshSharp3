using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public PolyCell MouseOverCell { get; private set; }
    public Action<PolyCell> ChangedCell { get; set; }
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
            .PolyCellGrid.GetElementAtPointWhere(mousePosMapSpace, 
                c => c is RiverCell,
                data);
        if(c == null) c = data.Planet.PolygonAux
            .PolyCellGrid.GetElementAtPoint(mousePosMapSpace, data);
        SetCell(c);

        if (c is ISinglePolyCell single)
        {
            SetPoly(single.Polygon.Entity(data));
        }
        else if (c is RiverCell r)
        {
            var edge = r.Edge.Entity(data);
            var p1 = edge.HighPoly.Entity(data);
            var p2 = edge.LowPoly.Entity(data);
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

    private void SetCell(PolyCell c)
    {
        if (c != MouseOverCell)
        {
            MouseOverCell = c;
            ChangedCell?.Invoke(MouseOverCell);
        }
    }
}
