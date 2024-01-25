
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
                new Color(front.Color, .5f));
        }
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
            var cell = face.GetNative(d);
            mb.AddPoint(relTo.GetOffsetTo(cell.GetCenter(), d),
                markerSize, color);
        }
        for (var i = 0; i < seg.FrontLineFaces.Count - 1; i++)
        {
            var face = seg.FrontLineFaces[i];
            var nextFace = seg.FrontLineFaces[i + 1];
            var from = face.GetNative(d);
            var to = nextFace.GetNative(d);
            
            mb.AddLine(relTo.GetOffsetTo(from.GetCenter(),d),
                relTo.GetOffsetTo(to.GetCenter(), d),
                color, markerSize);
        }
        
        
        for (var i = 0; i < seg.FrontLineFaces.Count; i++)
        {
            var face = seg.FrontLineFaces[i];
            var covering = seg.FrontFaceGroupIds[i];
            if (covering == -1) continue;
            var coveringGroup = d.Get<UnitGroup>(covering);
            var native = face.GetNative(d);
            var foreign = face.GetForeign(d);
            mb.AddArrow(relTo.GetOffsetTo(native.GetCenter(),d),
                relTo.GetOffsetTo(foreign.GetCenter(), d),
                markerSize / 5f, coveringGroup.Color);
        }

        var group = -1;
        var groupStart = -1;
        for (var i = 0; i < seg.FrontLineFaces.Count; i++)
        {
            var covering = seg.FrontFaceGroupIds[i];
            if (covering != group)
            {
                drawGroupLine(group, groupStart, i - 1);
                group = covering;
                groupStart = i;
            }
            if (i == seg.FrontLineFaces.Count - 1)
            {
                drawGroupLine(group, groupStart, i);
            }
        }
        
        foreach (var kvp in seg.InsertingGroups)
        {
            var insertingGroup = d.Get<UnitGroup>(kvp.Key);
            if (kvp.Value.HasValue == false) continue;
            var native = kvp.Value.Value.GetNative(d);
            var foreign = kvp.Value.Value.GetForeign(d);
            var offset = native.GetCenter().GetOffsetTo(foreign.GetCenter(), d);
            var nativePos = relTo.GetOffsetTo(native.GetCenter(), d);
            mb.AddArrow(nativePos - offset, nativePos, 10f, insertingGroup.Color);
        }

        void drawGroupLine(int groupId, int groupFrom, int groupTo)
        {
            if (groupId == -1) return;
            var group = d.Get<UnitGroup>(groupId);
            var line = seg.FrontLineFaces.GetRange(groupFrom, groupTo - groupFrom + 1);
            for (var i = 0; i < line.Count - 1; i++)
            {
                var a = line[i].GetNative(d);
                var b = line[i + 1].GetNative(d);
                if (a == b) continue;
                
                mb.AddLine(relTo.GetOffsetTo(a.GetCenter(), d),
                    relTo.GetOffsetTo(b.GetCenter(), d),
                    group.Color, markerSize / 2f);
            }
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
            var native = face.GetNative(d);
            var foreign = face.GetForeign(d);
            if (i < order.Faces.Count - 1)
            {
                var nextFace = order.Faces[i + 1];
                var nextNative = nextFace.GetNative(d);
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
    public static void DrawCellBorders(this MeshBuilder mb,
        Vector2 relTo, PolyCell cell, Data data)
    {
        for (var i = 0; i < cell.RelBoundary.Length; i++)
        {
            mb.AddLine(
                relTo.GetOffsetTo(cell.RelBoundary[i] + cell.RelTo, data), 
                relTo.GetOffsetTo(cell.RelBoundary.Modulo(i + 1) + cell.RelTo, data),
                Colors.Black, 1f);
        }
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