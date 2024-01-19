
using System.Linq;
using Godot;

public static class MeshBuilderExt
{
    public static void DrawFront(this MeshBuilder mb,
        Vector2 relTo, FrontAssignment front, Data d)
    {
        foreach (var c in front.GetCells(d))
        {
            mb.DrawPolygon(c.RelBoundary.Select(p => relTo.GetOffsetTo(p + c.RelTo, d)).ToArray(),
                front.Color);
        }
        
        // var line = FrontFinder.FindFrontSimple(
        //     front.GetCells(d),
        //     c => c.GetNeighbors(d),
        //     (p, q) => p.GetCenter().GetOffsetTo(q.GetCenter(), d),
        //     c => c.Id
        // );
        // for (var i = 0; i < line.Count; i++)
        // {
        //     var edge = line[i];
        //     var from = PlanetDomainExt.GetPolyCell(edge.X, d);
        //     var to = PlanetDomainExt.GetPolyCell(edge.Y, d);
        //     mb.AddLine(relTo.GetOffsetTo(from.GetCenter(), d),
        //         relTo.GetOffsetTo(to.GetCenter(), d),
        //         front.Color.Inverted(), 10f);
        // }
    }
    public static void DrawFrontSegment(this MeshBuilder mb,
        Vector2 relTo,
        FrontSegmentAssignment seg, 
        Data d)
    {
        var markerSize = 5f;
        var color = seg.Color;
        if (seg.FrontLineFaces.Count == 1)
        {
            var face = seg.FrontLineFaces[0];
            var cell = PlanetDomainExt.GetPolyCell(face.nativeId, d);
            mb.AddPoint(relTo.GetOffsetTo(cell.GetCenter(), d),
                markerSize, color);
        }
        for (var i = 0; i < seg.FrontLineFaces.Count - 1; i++)
        {
            var face = seg.FrontLineFaces[i];
            var nextFace = seg.FrontLineFaces[i + 1];
            var from = PlanetDomainExt.GetPolyCell(face.nativeId, d);
            var to = PlanetDomainExt.GetPolyCell(nextFace.nativeId, d);
            
            mb.AddLine(relTo.GetOffsetTo(from.GetCenter(),d),
                relTo.GetOffsetTo(to.GetCenter(), d),
                color, markerSize);
        }
        
        
        for (var i = 0; i < seg.FrontLineFaces.Count; i++)
        {
            var face = seg.FrontLineFaces[i];
            var native = PlanetDomainExt.GetPolyCell(face.nativeId, d);
            var foreign = PlanetDomainExt.GetPolyCell(face.foreignId, d);
            mb.AddArrow(relTo.GetOffsetTo(native.GetCenter(),d),
                relTo.GetOffsetTo(foreign.GetCenter(), d),
                markerSize / 5f, color);
        }
    }

    public static void DrawLineOrder(this MeshBuilder mb,
        Vector2 relTo, DeployOnLineGroupOrder order, UnitGroup group, Data d)
    {
        var markerSize = 2.5f;
        var color = group.Color;
        for (var i = 0; i < order.Faces.Count; i++)
        {
            var face = order.Faces[i];
            var native = PlanetDomainExt.GetPolyCell(face.nativeId, d);
            var foreign = PlanetDomainExt.GetPolyCell(face.foreignId, d);
            if (i < order.Faces.Count - 1)
            {
                var nextFace = order.Faces[i + 1];
                var nextNative = PlanetDomainExt.GetPolyCell(nextFace.nativeId, d);
                mb.AddLine(relTo.GetOffsetTo(native.GetCenter(),d),
                    relTo.GetOffsetTo(nextNative.GetCenter(), d),
                    color, markerSize);
            }
            mb.AddArrow(relTo.GetOffsetTo(native.GetCenter(),d),
                relTo.GetOffsetTo(foreign.GetCenter(), d),
                markerSize / 5f, color);
        }
    }
    public static void DrawPolyBorders(this MeshBuilder mb,
        Vector2 relTo, MapPolygon poly, Data data)
    {
        var edgeBorders = poly
            .GetOrderedBoundarySegs(data)
            .Select(s => s.Translate(relTo.GetOffsetTo(poly.Center, data)));
        mb.AddLines(edgeBorders.ToList(), 2f, Colors.Black);
    }

    public static void DrawMovementRecord(this MeshBuilder mb,
        int id, int howFarBack, Vector2 relTo, Data d)
    {
        var records = d.Context.MovementRecords;
        if (records.ContainsKey(id))
        {
            var last = records[id]
                .TakeLast(howFarBack).ToList();
            if (last.Count() == 0) return;
            var tick = last[0].tick;
            var tickIter = 0;
            for (var i = 0; i < last.Count - 1; i++)
            {
                var from = last[i];
                var fromCell = PlanetDomainExt.GetPolyCell(from.cellId, d);
                var to = last[i + 1];
                var toCell = PlanetDomainExt.GetPolyCell(to.cellId, d);

                if (to.tick != tick)
                {
                    tick = to.tick;
                    tickIter++;
                }

                var color = ColorsExt.GetRainbowColor(tickIter);
                mb.AddArrow(relTo.GetOffsetTo(fromCell.GetCenter(), d), 
                    relTo.GetOffsetTo(toCell.GetCenter(), d), 2f, color);
            }
        }
    }

    public static void DrawPolygon(this MeshBuilder mb,
        Vector2[] boundaryPoints, Color color)
    {
        var tris = Geometry2D.TriangulatePolygon(boundaryPoints);
        for (var i = 0; i < tris.Length; i+=3)
        {
            var p1 = boundaryPoints[tris[i]];
            var p2 = boundaryPoints[tris[i+1]];
            var p3 = boundaryPoints[tris[i+2]];
            // mb.AddTri(p1, p2, p3, lf.Color);
            // mb.AddTri(p1, p2, p3, v.Color);
            mb.AddTri(p1, p2, p3, color);
        }
    }
}