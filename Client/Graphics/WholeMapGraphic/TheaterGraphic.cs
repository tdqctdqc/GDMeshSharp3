
using System;
using System.Linq;
using Godot;

public partial class TheaterGraphic : Node2D
{
    public TheaterGraphic(TheaterBranch theaterBranch, 
        GraphicsSegmenter segmenter, Data d)
    {
        Draw(theaterBranch, d);
        var relTo = theaterBranch.Theater.Cells.First().GetCenter();
        segmenter.AddElement(this, relTo);
    }

    public void Draw(TheaterBranch theaterBranch, Data d)
    {
        this.ClearChildren();
        if (theaterBranch.Theater.Cells.Count() == 0) return;
        var alliance = theaterBranch.Regime.GetAlliance(d);
        var outer = theaterBranch.Regime.PrimaryColor;
        var inner = ColorsExt.GetRandomColor();
        
        var mb = MeshBuilder.GetFromPool();
        var color = ColorsExt.GetRandomColor();
        DrawTheaterWps(theaterBranch, mb, color, d);
        // DrawLinks(theater, mb, d);
        DrawFronts(theaterBranch, d, mb, color);
        mb.Return();
    }

    private void DrawFronts(TheaterBranch theaterBranch, 
        Data d, MeshBuilder mb, Color theaterColor)
    {
        var relTo = theaterBranch.Theater.Cells.First().GetCenter();
        Func<Vector2, Vector2> relPos = p => relTo.Offset(p, d);
        var regimeColor = theaterBranch.Regime.PrimaryColor;
        
        foreach (var seg in theaterBranch.Assignments.OfType<HoldLineAssignment>())
        {
            var segColor = seg.Color;
            for (var i = 0; i < seg.Frontline.Faces.Count - 1; i++)
            {
                var fromWp = seg.Frontline.Faces[i].GetNative(d);
                var toWp = seg.Frontline.Faces[i + 1].GetNative(d);
                var from = relPos(fromWp.GetCenter());
                var to = relPos(toWp.GetCenter());
                mb.AddLine(from, to, regimeColor, 5f);
                mb.AddLine(from, to, segColor, 1f);
            }
        }

        if (mb.Tris.Count > 0)
        {
            AddChild(mb.GetMeshInstance());
        }
    }

    private void DrawTheaterWps(TheaterBranch theaterBranch, 
        MeshBuilder mb, Color theaterColor, Data d)
    {
        var relTo = theaterBranch.Theater.Cells.First().GetCenter();
        var regimeColor = theaterBranch.Regime.PrimaryColor;
        var size = 5f;
        
        foreach (var wp in theaterBranch.Theater.Cells)
        {
            var relPos = relTo.Offset(wp.GetCenter(), d);
            mb.AddPoint(relPos, size, regimeColor);
            mb.AddPoint(relPos, size * .6f, theaterColor);
        }
    }

    private void DrawLinks(TheaterBranch theaterBranch, MeshBuilder mb, Data d)
    {
        var relTo = theaterBranch.Theater.Cells.First().GetCenter();
        var regimeColor = theaterBranch.Regime.PrimaryColor;

        foreach (var wp in theaterBranch.Theater.Cells)
        {
            var relPos = relTo.Offset(wp.GetCenter(), d);
            
            foreach (var nWp in wp.GetNeighbors(d))
            {
                if (theaterBranch.Theater.Cells.Contains(nWp) == false) continue;
                if (nWp.Id > wp.Id) continue;
                var nRelPos = relTo.Offset(nWp.GetCenter(), d);
        
                mb.AddLine(relPos, nRelPos, regimeColor, 2f);
            }
        }
    }
    
}