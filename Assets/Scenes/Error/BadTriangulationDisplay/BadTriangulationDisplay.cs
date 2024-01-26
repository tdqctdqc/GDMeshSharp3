using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public partial class BadTriangulationDisplay : Node2D
{
    public void Setup(BadTriangulationException err)
    {
        var label = (Label) FindChild("Label");
        label.Text = err.Poly.Id + " at " + err.Poly.Center;
        
        var mb = MeshBuilder.GetFromPool();
        mb.AddPointMarkers(new List<Vector2>{Vector2.Zero}, 10f, Colors.Red);
        mb.AddPointMarkers(new List<Vector2>{err.Poly.GetOrderedBoundarySegs(err.Data).Average()}, 10f, Colors.Green);
        for (var i = 0; i < err.Tris.Count; i++)
        {
            var inscribe = err.Tris[i].GetInscribed(1f);
            mb.AddTri(inscribe, err.Colors[i]);
        }
        for (var i = 0; i < err.Outlines.Count; i++)
        {
            var segs = err.Outlines[i];
            var col = ColorsExt.GetRainbowColor(i);
            for (var j = 0; j < segs.Count; j++)
            {
                mb.AddArrow(segs[j].From, 
                    segs[j].To, 1f, 
                    col
                );
            }
            mb.AddNumMarkers(segs.Select(s => s.Mid()).ToList(), 
                10f, Colors.Transparent, Colors.White, Vector2.Zero);

        }
        
        AddChild(mb.GetMeshInstance());
        mb.Return();
    }
}