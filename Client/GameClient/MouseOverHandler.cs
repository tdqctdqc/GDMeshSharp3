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
                FindPoly(data, mousePos);
                FindCell(MouseOverPoly, data, mousePos);
            });
    }
    
    public void Process(float delta)
    {
        _timerAction.Process(delta);
    }

    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {
        if (mousePosMapSpace.Y <= 0f || mousePosMapSpace.Y >= data.Planet.Height)
        {
            SetPoly(null);
            SetCell(null);
            return;
        }
        else if (MouseOverPoly != null && MouseOverPoly.PointInPolyAbs(mousePosMapSpace, data))
        {
            if (MouseOverCell != null && MouseOverCell.ContainsPoint(mousePosMapSpace, data))
            {
                return;
            }
        }
        else if (MouseOverPoly != null && 
                 MouseOverPoly.Neighbors.Items(data)
                         .FirstOrDefault(n => n.PointInPolyAbs(mousePosMapSpace, data))
                     is MapPolygon neighbor)
        {
            SetPoly(neighbor);
        }
        else
        {
            var p = data.Planet.PolygonAux.MapPolyGrid
                .GetElementAtPoint(mousePosMapSpace, data);
            SetPoly(p);
        }
    }

    
    private void FindCell(MapPolygon p, Data data, Vector2 mousePosMapSpace)
    {
        if (MouseOverPoly == null)
        {
            SetCell(null);
            return;
        }

        var c = data.Planet.PolygonAux.PolyCellGrid.GetElementAtPoint(mousePosMapSpace, data);
        SetCell(c);
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
