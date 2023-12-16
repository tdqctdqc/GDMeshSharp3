
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
        var relTo = MilitaryDomain.GetTacWaypoint(relToId, d).Pos;
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
        var relTo = MilitaryDomain.GetTacWaypoint(relToId, d).Pos;
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
        var relTo = MilitaryDomain.GetTacWaypoint(relToId, d).Pos;
        Func<Vector2, Vector2> relPos = p => relTo.GetOffsetTo(p, d);
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;
        foreach (var fa in theater.Fronts)
        {
            var iter = 0;
            var frontColor = ColorsExt.GetRandomColor();
            
            foreach (var seg in fa.Segments)
            {
                var segColor = ColorsExt.GetRandomColor();
                foreach (var wp in seg.GetTacWaypoints(d))
                {
                    mb.AddSquare(relPos(wp.Pos), 20f, regimeColor);
                    mb.AddSquare(relPos(wp.Pos), 15f, theaterColor);
                    mb.AddSquare(relPos(wp.Pos), 7.5f, segColor);
                }
            }
            foreach (var line in fa.GetLines(d))
            {
                var rainbow = ColorsExt.GetRainbowColor(iter);
                iter++;
                for (var i = 0; i < line.Count - 1; i++)
                {
                    mb.AddLine(relPos(line[i]), relPos(line[i + 1]), regimeColor, 5f);
                    mb.AddLine(relPos(line[i]), relPos(line[i + 1]), frontColor, 2.5f);
                    mb.AddLine(relPos(line[i]), relPos(line[i + 1]), rainbow, 1f);
                    // mb.AddSquare(relPos(line[i]), 5f, regimeColor);
                    // mb.AddSquare(relPos(line[i]), 2.5f, Colors.Pink);
                }
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
        var relTo = MilitaryDomain.GetTacWaypoint(relToId, d).Pos;
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;
        
        
        foreach (var tId in theater.TacWaypointIds)
        {
            var wp = MilitaryDomain.GetTacWaypoint(tId, d);
            var relPos = relTo.GetOffsetTo(wp.Pos, d);
            mb.AddSquare(relPos, 10f, regimeColor);
            mb.AddSquare(relPos, 6f, theaterColor);
        }
    }

    private void DrawLinks(TheaterAssignment theater, MeshBuilder mb, Data d)
    {
        var relToId = theater.TacWaypointIds.First();
        var relTo = MilitaryDomain.GetTacWaypoint(relToId, d).Pos;
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;

        foreach (var tId in theater.TacWaypointIds)
        {
            var wp = MilitaryDomain.GetTacWaypoint(tId, d);
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