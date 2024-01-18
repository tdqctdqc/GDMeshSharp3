using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public PolyCell MouseOverCell { get; private set; }
    public MouseOverHandler()
    {
    }
    
    public void Process(Data data, Vector2 mousePosMapSpace)
    {
        FindPoly(data, mousePosMapSpace);
        FindCell(MouseOverPoly, data, mousePosMapSpace);
    }

    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {

        if (mousePosMapSpace.Y <= 0f || mousePosMapSpace.Y >= data.Planet.Height)
        {
            MouseOverPoly = null;
            MouseOverCell = null;
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
            MouseOverPoly = neighbor;
        }
        else
        {
            var p = data.Planet.PolygonAux.MapPolyGrid
                .GetElementAtPoint(mousePosMapSpace, data);
            if (p == null) return;
            MouseOverPoly = p;
        }
    }
    private void FindCell(MapPolygon p, Data data, Vector2 mousePosMapSpace)
    {
        if (MouseOverPoly == null)
        {
            MouseOverCell = null;
            return;
        }

        MouseOverCell = data.Planet.PolygonAux.PolyCellGrid.GetElementAtPoint(mousePosMapSpace, data);
    }
}
