using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverPolyHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public PolyTri MouseOverTri { get; private set; }
    private PolyTooltipTemplate _polyTemplate;
    private TacWaypointTooltipTemplate _waypointTemplate;
    public MouseOverPolyHandler()
    {
        // _template = new PolyTooltipTemplate();
        _polyTemplate = new PolyTooltipTemplate();
        _waypointTemplate = new TacWaypointTooltipTemplate();
    }
    
    public void Process(float delta, Data data, Vector2 mousePosMapSpace)
    {
        FindPoly(data, mousePosMapSpace);
        // DrawPolyTooltip();
        FindWaypoint(data, mousePosMapSpace);
        
    }

    private void FindWaypoint(Data data, Vector2 mousePosMapSpace)
    {
        if (data.Military == null) throw new Exception();
        if (data.Military.WaypointGrid == null) throw new Exception();
        if (data.Military.WaypointGrid.TryGetClosest(mousePosMapSpace, out var wp, wp => true))
        {
            Game.I.Client.GetComponent<TooltipManager>()
                .PromptTooltip(_waypointTemplate, wp, GetHashCode());
            
        }
    }
    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {

        if (mousePosMapSpace.Y <= 0f || mousePosMapSpace.Y >= data.Planet.Height)
        {
            MouseOverPoly = null;
            MouseOverTri = null;
            Game.I.Client.GetComponent<TooltipManager>().HideTooltip(GetHashCode());
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
            var p = data.Planet.PolygonAux.MapPolyGrid.GetElementAtPoint(mousePosMapSpace, data);
            if (p == null) return;
            MouseOverPoly = p;
        }
        FindTri(MouseOverPoly, data, mousePosMapSpace);
        if(MouseOverTri != null)
        {
            var pos = new PolyTriPosition(MouseOverPoly.Id, MouseOverTri.Index);
            Game.I.Client.UiRequests.MouseOver.Invoke(pos);
        }
        
    }

    private void DrawPolyTooltip()
    {
        if(MouseOverTri != null)
        {
            var pos = new PolyTriPosition(MouseOverPoly.Id, MouseOverTri.Index);
            Game.I.Client.UiRequests.MouseOver.Invoke(pos);
            Game.I.Client.GetComponent<TooltipManager>()
                .PromptTooltip(_polyTemplate, pos, GetHashCode());
        }
    }
    private void FindTri(MapPolygon p, Data data,  Vector2 mousePosMapSpace)
    {
        var offset = MouseOverPoly.GetOffsetTo(mousePosMapSpace, data);
        MouseOverTri = MouseOverPoly.Tris.GetAtPoint(offset, data);
    }
}
