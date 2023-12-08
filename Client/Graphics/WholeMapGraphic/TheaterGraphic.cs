
using System;
using System.Linq;
using Godot;

public partial class TheaterGraphic : Node2D
{
    public TheaterGraphic(TheaterAssignment theater, 
        GraphicsSegmenter segmenter, Data d)
    {
        Draw(theater, d);
        var relToId = theater.TacWaypointIds.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        segmenter.AddElement(this, relTo);
    }

    public void  Draw(TheaterAssignment theater, Data d)
    {
        this.ClearChildren();
        if (theater.TacWaypointIds.Count() == 0) return;
        var alliance = theater.Regime.Entity(d).GetAlliance(d);
        var outer = theater.Regime.Entity(d).PrimaryColor;
        var inner = ColorsExt.GetRandomColor();
        
        var relToId = theater.TacWaypointIds.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        var mb = new MeshBuilder();
        var color = ColorsExt.GetRandomColor();
        DrawTheaterWps(theater, mb, color, d);
        // DrawLinks(theater, mb, d);
        DrawFronts(theater, d, mb, color);
    }

    private void DrawFronts(TheaterAssignment theater, 
        Data d, MeshBuilder mb, Color theaterColor)
    {
        var relToId = theater.TacWaypointIds.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        Func<Vector2, Vector2> relPos = p => relTo.GetOffsetTo(p, d);
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;
        foreach (var fa in theater.Fronts)
        {
            foreach (var seg in fa.Segments)
            {
                // var segColor = ColorsExt.GetRandomColor();
                
                foreach (var wp in seg.GetHeldWaypoints(d))
                {
                    mb.AddSquare(relPos(wp.Pos), 20f, regimeColor);
                    mb.AddSquare(relPos(wp.Pos), 15f, theaterColor);
                }
                
                mb.AddLine(relPos(seg.LeftJoin), relPos(seg.Left), regimeColor, 10f);
                mb.AddLine(relPos(seg.LeftJoin), relPos(seg.Left), theaterColor, 5f);
                
                mb.AddLine(relPos(seg.Left), relPos(seg.Center), regimeColor, 10f);
                mb.AddLine(relPos(seg.Left), relPos(seg.Center), theaterColor, 5f);
                
                mb.AddLine(relPos(seg.Center), relPos(seg.Right), regimeColor, 10f);
                mb.AddLine(relPos(seg.Center), relPos(seg.Right), theaterColor, 5f);

                mb.AddLine(relPos(seg.Right), relPos(seg.RightJoin), regimeColor, 10f);
                mb.AddLine(relPos(seg.Right), relPos(seg.RightJoin), theaterColor, 5f);
                
                
            }
        }

        if (mb.Tris.Count > 0)
        {
            AddChild(mb.GetMeshInstance());
        }
    }

    private void DrawTheaterWps(TheaterAssignment theater, 
        MeshBuilder mb, Color theaterColor, Data d)
    {
        var relToId = theater.TacWaypointIds.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;
        
        
        foreach (var tId in theater.TacWaypointIds)
        {
            var wp = d.Military.TacticalWaypoints.Waypoints[tId];
            var relPos = relTo.GetOffsetTo(wp.Pos, d);
            mb.AddSquare(relPos, 10f, regimeColor);
            mb.AddSquare(relPos, 6f, theaterColor);
        }
    }

    private void DrawLinks(TheaterAssignment theater, MeshBuilder mb, Data d)
    {
        var relToId = theater.TacWaypointIds.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;

        foreach (var tId in theater.TacWaypointIds)
        {
            var wp = d.Military.TacticalWaypoints.Waypoints[tId];
            var relPos = relTo.GetOffsetTo(wp.Pos, d);
            
            foreach (var nWp in wp.TacNeighbors(d))
            {
                if (theater.TacWaypointIds.Contains(nWp.Id) == false) continue;
                if (nWp.Id > tId) continue;
                var nRelPos = relTo.GetOffsetTo(nWp.Pos, d);
        
                mb.AddLine(relPos, nRelPos, regimeColor, 2f);
            }
        }
    }
    
}