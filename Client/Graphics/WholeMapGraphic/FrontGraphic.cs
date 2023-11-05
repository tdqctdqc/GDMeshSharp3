
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class FrontGraphic : Node2D
{
    private int _currSegment = -1;
    private FrontGraphic() {}
    public Node2D Front { get; private set; } 
    public Node2D Line { get; private set; }

    public FrontGraphic(Front front, GraphicsSegmenter segmenter, Data data)
    {
        Draw(front, segmenter, data);
        _currSegment = segmenter.AddElement
            (this, front.RelTo(data));
    }

    private void Draw(Front front, GraphicsSegmenter segmenter, Data data)
    {
        var relTo = front.RelTo(data);
        var regime = front.Regime.Entity(data);
        
        DrawFrontWithFill(regime, front, relTo, data);
        // DrawFrontWithLines(regime, front, relTo, data);
        DrawFrontline(regime, front, relTo, data);
        
    }

    private void DrawFrontWithFill(Regime regime, Front front,
        Vector2 relTo, Data data)
    {
        if (Front != null)
        {
            Front.Free();
            Front = null;
        }
        var mb = new MeshBuilder();
        var offsets = front.GetWaypoints(data).Select(wp => data.Planet.GetOffsetTo(relTo, wp.Pos)).ToList();

        var fillColor = new Color(regime.PrimaryColor, .75f);
        if (front.WaypointIds.Count() == 1)
        {
            var wp = data.Planet.Nav.Waypoints[front.WaypointIds.First()];
            mb.AddCircle(data.Planet.GetOffsetTo(relTo, wp.Pos), 
                25f, 12, fillColor);
        }
        else if (front.WaypointIds.Count() == 2)
        {
            var wp1 = data.Planet.Nav.Waypoints[front.WaypointIds.ElementAt(0)];
            var wp2 = data.Planet.Nav.Waypoints[front.WaypointIds.ElementAt(1)];

            mb.AddLine(data.Planet.GetOffsetTo(relTo, wp1.Pos),
                data.Planet.GetOffsetTo(relTo, wp2.Pos),
                fillColor, 25f);
        }
        else if (front.WaypointIds.Count() > 2)
        {
            var tris = Triangulator
                .TriangulatePoints(offsets);
            foreach (var triangle in tris)
            {
                mb.AddTri(triangle, fillColor);
            }
        }

        if (mb.Tris.Count > 0)
        {
            Front = mb.GetMeshInstance();
            AddChild(Front);
        }
    }

    private void DrawFrontWithLines(Regime regime, Front front,
        Vector2 relTo, Data data)
    {
        if (Front != null)
        {
            Front.Free();
            Front = null;
        }
        var mb = new MeshBuilder();
        foreach (var wp in front.GetWaypoints(data))
        {
            var p = data.Planet.GetOffsetTo(relTo, wp.Pos);
            foreach (var nWp in wp.GetNeighboringWaypoints(data))
            {
                if (wp.Id < nWp.Id) continue;
                if (front.WaypointIds.Contains(nWp.Id) == false) continue;
                var nP = data.Planet.GetOffsetTo(relTo, nWp.Pos);
                mb.AddLine(p, nP, regime.PrimaryColor, 5f);
            }
        }
        if (mb.Tris.Count > 0)
        {
            Front = mb.GetMeshInstance();
            AddChild(Front);
        }
    }

    private void DrawFrontline(Regime regime, Front front, Vector2 relTo, 
        Data data)
    {
        if (Line != null)
        {
            Line.Free();
            Line = null;
        }
        var frontlines = front.GetFrontlines(data);
        var lineColor = regime.PrimaryColor.Darkened(.3f);
        var mb = new MeshBuilder();
        foreach (var frontline in frontlines)
        {
            if (frontline.Count == 1)
            {
                var pos = data.Planet.GetOffsetTo(relTo, frontline.First().Pos);
                mb.AddCircle(pos, 30f, 12, regime.PrimaryColor);
            }
            else
            {
                for (var i = 0; i < frontline.Count - 1; i++)
                {
                    var from = data.Planet.GetOffsetTo(relTo, frontline[i].Pos);
                    var to = data.Planet.GetOffsetTo(relTo, frontline[i + 1].Pos);;
                    mb.AddLine(from, to, lineColor, 25f);
                }
            }
        }
        if (mb.Tris.Count > 0)
        {
            Line = mb.GetMeshInstance();
            AddChild(Line);
        }
    }
    public void Update(Front front, Data data, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
        queue.Enqueue(() =>
        {
            if(GetParent() is Node n) n.RemoveChild(this);
            Draw(front, segmenter, data);
            _currSegment = segmenter.SwitchSegments(this, front.RelTo(data), _currSegment);
        });
    }
}