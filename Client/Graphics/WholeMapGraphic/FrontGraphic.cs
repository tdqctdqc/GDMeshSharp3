
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class FrontGraphic : Node2D
{
    private FrontGraphic() {}
    public Front Front { get; private set; }
    public Node2D FrontNode { get; private set; } 
    public Node2D LineNode { get; private set; }

    public FrontGraphic(Front front, GraphicsSegmenter segmenter, Data data)
    {
        Front = front;
        Draw(front, segmenter, data);
        segmenter.AddElement(this, front.RelTo(data));
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
        if (FrontNode != null)
        {
            FrontNode.Free();
            FrontNode = null;
        }
        var mb = new MeshBuilder();
        var offsets = front.GetHeldWaypoints(data)
            .Select(wp => relTo.GetOffsetTo(wp.Pos, data)).ToList();

        var fillColor = new Color(regime.PrimaryColor, .75f);
        if (front.HeldWaypointIds.Count() == 1)
        {
            var wp = data.Military.TacticalWaypoints.Waypoints[front.HeldWaypointIds.First()];
            mb.AddCircle(relTo.GetOffsetTo(wp.Pos, data), 
                25f, 12, fillColor);
        }
        else if (front.HeldWaypointIds.Count() == 2)
        {
            var wp1 = data.Military.TacticalWaypoints.Waypoints[front.HeldWaypointIds.ElementAt(0)];
            var wp2 = data.Military.TacticalWaypoints.Waypoints[front.HeldWaypointIds.ElementAt(1)];

            mb.AddLine(relTo.GetOffsetTo(wp1.Pos, data),
                relTo.GetOffsetTo(wp2.Pos, data),
                fillColor, 25f);
        }
        else if (front.HeldWaypointIds.Count() > 2
                 & offsets.Count > 2)
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
            FrontNode = mb.GetMeshInstance();
            AddChild(FrontNode);
        }
    }

    private void DrawFrontWithLines(Regime regime, Front front,
        Vector2 relTo, Data data)
    {
        if (FrontNode != null)
        {
            FrontNode.Free();
            FrontNode = null;
        }
        var mb = new MeshBuilder();
        foreach (var wp in front.GetHeldWaypoints(data))
        {
            var p = relTo.GetOffsetTo(wp.Pos, data);
            foreach (var nWp in wp.TacNeighbors(data))
            {
                if (wp.Id < nWp.Id) continue;
                if (front.HeldWaypointIds.Contains(nWp.Id) == false) continue;
                var nP = relTo.GetOffsetTo(nWp.Pos, data);
                mb.AddLine(p, nP, regime.PrimaryColor, 5f);
            }
        }
        if (mb.Tris.Count > 0)
        {
            FrontNode = mb.GetMeshInstance();
            AddChild(FrontNode);
        }
    }

    private void DrawFrontline(Regime regime, Front front, Vector2 relTo, 
        Data data)
    {
        if (LineNode != null)
        {
            LineNode.Free();
            LineNode = null;
        }
        var frontline = front
            .HeldWaypointIds;
        var darkened = regime.PrimaryColor.Darkened(.3f);
        var frontColor = ColorsExt.GetRandomColor();
        var mb = new MeshBuilder();
        if (frontline.Count == 1)
        {
            // var firstWp = data.Military.TacticalWaypoints.Waypoints[frontline[0]];
            //
            // var pos = data.Planet.GetOffsetTo(relTo, firstWp.Pos);
            // mb.AddCircle(pos, 30f, 12, regime.PrimaryColor);
        }
        else
        {
            // var iter = 0;
            // for (var i = 0; i < frontline.Count - 1; i++)
            // {
            //     var fromWp = data.Military.TacticalWaypoints.Waypoints[frontline[i]];
            //     var toWp = data.Military.TacticalWaypoints.Waypoints[frontline[i + 1]];
            //     var from = data.Planet.GetOffsetTo(relTo, fromWp.Pos);
            //     var to = data.Planet.GetOffsetTo(relTo, toWp.Pos);;
            //     mb.AddLine(from, to, 
            //         darkened, 
            //         15f);
            //     mb.AddLine(from, to, 
            //         frontColor, 
            //         10f);
            //     iter++;
            // }
        }
        if (mb.Tris.Count > 0)
        {
            LineNode = mb.GetMeshInstance();
        }
        else
        {
            LineNode = new Node2D();
        }
        AddChild(LineNode);
    }
    public void Update(Front front, Data data, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
    }
}