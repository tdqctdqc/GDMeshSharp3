using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverPolyHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public PolyTri MouseOverTri { get; private set; }
    private DataTooltipInstance<PolyTriPosition> _instance;
    public MouseOverPolyHandler()
    {
        _instance = new DataTooltipInstance<PolyTriPosition>(new PolyDataTooltipTemplate(), 
            new PolyTriPosition(-1, (byte)255));
    }
    public void Process(float delta, Data data, Vector2 mousePosMapSpace)
    {
        FindPoly(data, mousePosMapSpace);
    }
    
    private void FindPoly(Data data, Vector2 mousePosMapSpace)
    {

        if (mousePosMapSpace.Y <= 0f || mousePosMapSpace.Y >= data.Planet.Height)
        {
            MouseOverPoly = null;
            MouseOverTri = null;
            Game.I.Client.UiRequests.HideTooltip.Invoke(_instance);
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
                 MouseOverPoly.Neighbors.Entities(data)
                         .FirstOrDefault(n => n.PointInPolyAbs(mousePosMapSpace, data))
                     is MapPolygon neighbor)
        {
            MouseOverPoly = neighbor;
        }
        else
        {
            var p = data.Planet.PolygonAux.MapPolyGrid.GetElementAtPoint(mousePosMapSpace);
            if (p == null) return;
            MouseOverPoly = p;
        }
        FindTri(MouseOverPoly, data, mousePosMapSpace);
        
        if(MouseOverTri != null)
        {
            var pos = new PolyTriPosition(MouseOverPoly.Id, MouseOverTri.Index);
            Game.I.Client.UiRequests.MouseOver.Invoke(pos);
            _instance.SetElement(pos);
            Game.I.Client.UiRequests.PromptTooltip.Invoke(_instance);
        }
        
    }
    private void FindTri(MapPolygon p, Data data,  Vector2 mousePosMapSpace)
    {
        var offset = MouseOverPoly.GetOffsetTo(mousePosMapSpace, data);
        MouseOverTri = MouseOverPoly.Tris.GetAtPoint(offset, data);
    }
}
