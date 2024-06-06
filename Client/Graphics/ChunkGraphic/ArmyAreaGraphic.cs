
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ArmyAreaGraphic : Node2D
{
    private MeshInstance2D _mesh;

    public ArmyAreaGraphic()
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.ArmyArea;
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
        mb.DrawCellsBordersInsetLocal(cells, 
            army.Regime.Get(c.Data).PrimaryColor.Tint(.25f),
            army.Color, 
            2f, 3f, army.GetHomeCell(c.Data).RelTo, c.Data);
        
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