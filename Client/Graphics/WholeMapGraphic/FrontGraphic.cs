
using System;
using System.Collections.Concurrent;
using System.Linq;
using Godot;

public partial class FrontGraphic : Node2D
{
    private int _currSegment = -1;
    private FrontGraphic() {}
    private Node _child;

    public FrontGraphic(Front front, GraphicsSegmenter segmenter, Data data)
    {
        Draw(front, segmenter, data);
        _currSegment = segmenter.AddElement(this, front.RelTo(data));
    }

    private void Draw(Front front, GraphicsSegmenter segmenter, Data data)
    {
        if(_child != null)
        {
            this.RemoveChild(_child);
            _child.QueueFree();
        }
        
        var relTo = front.RelTo(data);
        var regime = front.Regime.Entity(data);
        var mb = new MeshBuilder();
        
        foreach (var fId1 in front.WaypointIds)
        {
            var wp1 = data.Planet.Nav.Waypoints[fId1];
            var p1 = wp1.Pos;
            p1 = data.Planet.GetOffsetTo(relTo, p1);
            
            foreach (var wp2 in wp1.GetNeighboringWaypoints(data))
            {
                if (front.WaypointIds.Contains(wp2.Id) == false) continue;
                var p2 = wp2.Pos;
                p2 = data.Planet.GetOffsetTo(relTo, p2);

                mb.AddLine(p1, p2, regime.SecondaryColor, 25f);
                mb.AddLine(p1, p2, regime.PrimaryColor, 20f);
            }
        }

        if (mb.Tris.Count() > 0)
        {
            //todo make representation for single waypoint fronts
            _child = mb.GetMeshInstance();
            AddChild(_child);
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