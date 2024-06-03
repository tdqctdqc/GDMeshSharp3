
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ArmyGraphic : Node2D
{
    private MeshInstance2D _mesh;

    public ArmyGraphic()
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.Units;
        _mesh = new MeshInstance2D();
        AddChild(_mesh);
    }
    public void Draw(Army army, Client c)
    {
        var regime = army.Regime.Get(c.Data);
        var homeCell = army.GetHomeCell(c.Data);
        
        var chunk = homeCell.GetChunk(c.Data);
        var segmenter = c.GetComponent<MapGraphics>().Segmenter;
        
        c.QueuedUpdates.Enqueue(() => segmenter.AddElement(this, homeCell.RelTo));
        
        var mb = MeshBuilder.GetFromPool();
        var thickness = 10f;
        var cells = army.GetCells(c.Data);
        // NewMethod(army, c, cells, mb, homeCell);


        OldMethod(army, c, cells, homeCell, mb, thickness, regime);
        var mesh = mb.GetMesh();
        c.QueuedUpdates.Enqueue(() =>
        {
            if (mesh is not null)
            {
                _mesh.Mesh = mesh;
            }
            else
            {
                _mesh.Mesh = null;
            }
        });

        mb.Return();
    }

    private void OldMethod(Army army, Client c, HashSet<Cell> cells, Cell homeCell, MeshBuilder mb, float thickness,
        Regime regime)
    {
        if (cells.Count > 1)
        {
            foreach (var cell in army.GetCells(c.Data))
            {
                var offset = homeCell.RelTo.Offset(cell.GetCenter(), c.Data);
                foreach (var nCell in cell.GetNeighbors(c.Data))
                {
                    if (nCell.Id > cell.Id
                        || army.Cells.Contains(nCell.Id) == false)
                    {
                        continue;
                    }

                    var nOffset = homeCell.RelTo.Offset(nCell.GetCenter(), c.Data);

                    mb.AddLine(offset, nOffset, army.Color, thickness);
                    mb.AddLine(offset, nOffset, regime.PrimaryColor, thickness / 2f);
                }
            }
        }
        else
        {
            var cell = cells.First();
            var offset = homeCell.RelTo.Offset(cell.GetCenter(), c.Data);
            mb.AddCircle(offset, thickness, 12, army.Color);
            mb.AddCircle(offset, thickness / 2f, 12, regime.PrimaryColor);
        }

        
    }

    private static void NewMethod(Army army, Client c, 
        HashSet<Cell> cells, MeshBuilder mb, Cell homeCell)
    {
        var unions = UnionFind.Find<Cell>(
            cells, (c1, c2) => true,
            c1 => c1.GetNeighbors(c.Data)
                .Where(c => cells.Contains(c) && c is not RiverCell))
            .Where(u => u.Count > 0);

        foreach (var union in unions)
        {
            var hash = union.ToHashSet();
            
            var fronts = FrontFinder.FindFront(
                    union.ToHashSet(),
                    c => union.Contains(c) == false,
                    c.Data
                )
                .Where(l => l.Count > 0)
                .Select(l =>
                {
                    var list = new List<LineSegment>();
                    for (var i = 0; i < l.Count; i++)
                    {
                        var face = l[i];
                        var native = face.GetNative(c.Data);
                        var foreign = face.GetForeign(c.Data);
                        var edgeRelToNative = native.GetEdgeRelWith(foreign);
                        var pRel1 = homeCell.RelTo.Offset(
                                edgeRelToNative.Item1 + native.RelTo,
                                c.Data);
                        var pRel2 = homeCell.RelTo.Offset(
                            edgeRelToNative.Item2 + native.RelTo,
                            c.Data);
                        list.Add(new LineSegment(pRel1, pRel2));
                    }
                    
                    return (l.First().Native, list.FlipChainify().GetPoints());
                });
            
            var colorIter = 0;
            
            foreach (var (id, front) in fronts)
            {
                var poly = front.ToArray();
                var insets = Geometry2D.OffsetPolygon(poly, -3f);
                foreach (var inset in insets)
                {
                    mb.DrawPolygon(inset, army.Color.Tint(.5f));
                    var insets2 = Geometry2D.OffsetPolygon(
                        inset, 
                        -3f);
                    foreach (var inset2 in insets2)
                    {
                        mb.DrawPolygon(inset2, 
                            army.Regime.Get(c.Data).PrimaryColor.Tint(.5f));
                    }
                }
            }
            
            
            if (fronts.Count() > 1)
            {
                GD.Print("fronts count " + fronts.Count()
                + " at " + fronts.First().Native);
            }
        }
    }
}