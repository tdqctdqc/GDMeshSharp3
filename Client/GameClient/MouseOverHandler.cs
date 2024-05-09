using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MouseOverHandler
{
    public MapPolygon MouseOverPoly { get; private set; }
    public Cell MouseOverCell { get; private set; }
    public Cell SecondClosest { get; private set; }
    public Action<Cell> ChangedCell { get; set; }
    public Action<MapPolygon> ChangedPoly { get; set; }
    public Action<Cell> ChangedSecondClosest { get; set; }
    private TimerAction _timerAction;
    private Func<Cell, bool> _validCell;
    public MouseOverHandler(Data data,
        Func<Cell, bool> validCell = null)
    {
        if (validCell is null)
        {
            _validCell = c => true;
        }
        else
        {
            _validCell = validCell;
        }
        
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
    
    private void Find(Data data, Vector2 mousePosMapSpace)
    {
        SetCell(data, mousePosMapSpace);
        SetClosests(data, mousePosMapSpace);
        SetPoly(data, mousePosMapSpace);
        
        ChangedCell?.Invoke(MouseOverCell);
        ChangedPoly?.Invoke(MouseOverPoly);
        ChangedSecondClosest?.Invoke(SecondClosest);
    }

    private void SetClosests(Data data, Vector2 mousePosMapSpace)
    {
        Cell secondClosestAny = null;
        
        if(MouseOverCell is not null)
        {
            var dist = Mathf.Inf;
            var relToCell = MouseOverCell.RelTo.Offset(mousePosMapSpace, data);

            for (var i = 0; i < MouseOverCell.Geometry.EdgesRel.Count; i++)
            {
                var edge = MouseOverCell.Geometry.EdgesRel[i];
                var thisDist = Vector2Ext.DistToLine(relToCell, edge.Item1, edge.Item2);
                
                if (thisDist < dist)
                {
                    var cand = PlanetDomainExt.GetPolyCell(MouseOverCell.Geometry.Neighbors[i], data);
                    if (_validCell(cand))
                    {
                        secondClosestAny = cand;
                        dist = thisDist;
                    }
                }
            }
        }

        if (secondClosestAny != SecondClosest)
        {
            SecondClosest = secondClosestAny;
        }
    }

    private void SetPoly(Data data, Vector2 mousePosMapSpace)
    {
        MapPolygon close = null;
        if (MouseOverCell is null)
        {
            close = null;
        }
        else if (MouseOverCell is IPolyCell single)
        {
            close = single.Polygon.Get(data);
        }
        else if (MouseOverCell is RiverCell r)
        {
            var edge = r.Edge.Get(data);
            var p1 = edge.HighPoly.Get(data);
            var p2 = edge.LowPoly.Get(data);
            close = mousePosMapSpace.Offset(p1.Center, data)
                       < mousePosMapSpace.Offset(p2.Center, data)
                ? p1 : p2;
        }
        
        
        MouseOverPoly = close;
    }

    private void SetCell(Data data, Vector2 mousePosMapSpace)
    {
        var c = data.Planet.MapAux
            .CellGrid.GetElementAtPointWhere(mousePosMapSpace, 
                c => c is RiverCell && _validCell(c),
                data);
        if(c == null) c = data.Planet.MapAux
            .CellGrid.GetElementAtPointWhere(mousePosMapSpace,
                _validCell,
                data);
        
        MouseOverCell = c;
    }


    public void Highlight()
    {
        var client = Game.I.Client;
        var highlight = client.GetComponent<MapGraphics>().Highlighter;
        client.HighlightCell(MouseOverCell, 2f);
        client.HighlightCellNeighbors(MouseOverCell, 1f);
        // client.HighlightPoly(MouseOverPoly, 1f);
        if (SecondClosest is not null)
        {
            var edge = MouseOverCell
                .GetEdgeRelWith(SecondClosest);
            highlight.Draw(mb => mb.AddLine(edge.Item1,
                edge.Item2, Colors.Blue, 2f), 
                MouseOverCell.RelTo);
        }
        else
        {
            GD.Print("Couldnt find any");
        }
    }
}
