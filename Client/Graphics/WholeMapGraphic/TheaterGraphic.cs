
using System;
using System.Linq;
using Godot;

public partial class TheaterGraphic : Node2D
{
    public TheaterGraphic(Theater theater, 
        GraphicsSegmenter segmenter, Data d)
    {
        Draw(theater, d);
        var relToId = theater.HeldCellIds.First();
        var relTo = PlanetDomainExt.GetPolyCell(relToId, d).GetCenter();
        segmenter.AddElement(this, relTo);
    }

    public void Draw(Theater theater, Data d)
    {
        this.ClearChildren();
        if (theater.HeldCellIds.Count() == 0) return;
        var alliance = theater.Regime.Entity(d).GetAlliance(d);
        var outer = theater.Regime.Entity(d).PrimaryColor;
        var inner = ColorsExt.GetRandomColor();
        
        var relToId = theater.HeldCellIds.First();
        var relTo = PlanetDomainExt.GetPolyCell(relToId, d).GetCenter();
        var mb = MeshBuilder.GetFromPool();
        var color = ColorsExt.GetRandomColor();
        DrawTheaterWps(theater, mb, color, d);
        // DrawLinks(theater, mb, d);
        DrawFronts(theater, d, mb, color);
        mb.Return();
    }

    private void DrawFronts(Theater theater, 
        Data d, MeshBuilder mb, Color theaterColor)
    {
        var relToId = theater.HeldCellIds.First();
        var relTo = PlanetDomainExt.GetPolyCell(relToId, d).GetCenter();
        Func<Vector2, Vector2> relPos = p => relTo.GetOffsetTo(p, d);
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;
        
        foreach (var seg in theater.Branches.OfType<FrontSegment>())
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

    private void DrawTheaterWps(Theater theater, 
        MeshBuilder mb, Color theaterColor, Data d)
    {
        var relToId = theater.HeldCellIds.First();
        var relTo = PlanetDomainExt.GetPolyCell(relToId, d).GetCenter();
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;
        var size = 5f;
        
        foreach (var tId in theater.HeldCellIds)
        {
            var wp = PlanetDomainExt.GetPolyCell(tId, d);
            var relPos = relTo.GetOffsetTo(wp.GetCenter(), d);
            mb.AddPoint(relPos, size, regimeColor);
            mb.AddPoint(relPos, size * .6f, theaterColor);
        }
    }

    private void DrawLinks(Theater theater, MeshBuilder mb, Data d)
    {
        var relToId = theater.HeldCellIds.First();
        var relTo = PlanetDomainExt.GetPolyCell(relToId, d).GetCenter();
        var regimeColor = theater.Regime.Entity(d).PrimaryColor;

        foreach (var tId in theater.HeldCellIds)
        {
            var wp = PlanetDomainExt.GetPolyCell(tId, d);
            var relPos = relTo.GetOffsetTo(wp.GetCenter(), d);
            
            foreach (var nWp in wp.GetNeighbors(d))
            {
                if (theater.HeldCellIds.Contains(nWp.Id) == false) continue;
                if (nWp.Id > tId) continue;
                var nRelPos = relTo.GetOffsetTo(nWp.GetCenter(), d);
        
                mb.AddLine(relPos, nRelPos, regimeColor, 2f);
            }
        }
    }
    
}