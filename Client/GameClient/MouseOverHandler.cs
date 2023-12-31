using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public PolyTri MouseOverTri { get; private set; }
    public Waypoint MouseOverWaypoint { get; private set; }
    public MouseOverHandler()
    {
    }
    
    public void Process(Data data, Vector2 mousePosMapSpace)
    {
        FindPoly(data, mousePosMapSpace);
        FindTri(MouseOverPoly, data, mousePosMapSpace);
        FindWaypoint(data, mousePosMapSpace);
    }

    private void FindWaypoint(Data data, Vector2 mousePosMapSpace)
    {
        MouseOverWaypoint = null;
        if (data.Military.WaypointGrid.TryGetClosest(mousePosMapSpace, 
                out var wp, 
                wp => true))
        {
            MouseOverWaypoint = wp;
        }
    }
    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {

        if (mousePosMapSpace.Y <= 0f || mousePosMapSpace.Y >= data.Planet.Height)
        {
            MouseOverPoly = null;
            MouseOverTri = null;
            return;
        }
        else if (MouseOverPoly != null && MouseOverPoly.PointInPolyAbs(mousePosMapSpace, data))
        {
            if (MouseOverTri != null && MouseOverTri.ContainsPoint(mousePosMapSpace - MouseOverPoly.Center))
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
    private void FindTri(MapPolygon p, Data data, Vector2 mousePosMapSpace)
    {
        if (MouseOverPoly == null)
        {
            MouseOverTri = null;
            return;
        }
        var offset = MouseOverPoly.GetOffsetTo(mousePosMapSpace, data);
        MouseOverTri = MouseOverPoly.Tris.GetAtPoint(offset, data);
    }
}
