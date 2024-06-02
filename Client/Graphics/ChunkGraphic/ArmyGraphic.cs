
using System;
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
        var unions = UnionFind.Find<Cell>(
            cells, (c1, c2) => true, c1 => c1.GetNeighbors(c.Data));
        
        
        
            
        
        
        
        
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
}