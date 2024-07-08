
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ArmyAreaGraphic : Node2D
{
    private MeshInstance2D _mesh;
    public Vector2 RelTo { get; private set; }
    public ArmyAreaGraphic()
    {
        ZAsRelative = false;
        ZIndex = (int)LayerOrder.ArmyArea;
    }

    public void Initialize()
    {
        _mesh = new MeshInstance2D();
        AddChild(_mesh);
    }
    public void Draw(Army army, Client c)
    {
        var regime = army.Regime.Get(c.Data);
        var homeCell = army.GetHomeCell(c.Data);
        RelTo = homeCell.RelTo;

        var mapGraphics = c.GetComponent<MapGraphics>();
        var segmenter = mapGraphics.Segmenter;
        var uiEls = mapGraphics.UiElements;
        var cells = army.GetCells(c.Data);
        if (cells.Count == 0) return;
        c.QueuedUpdates.Enqueue(
            () => segmenter.AddElement(this, RelTo));
        
        var mb = MeshBuilder.GetFromPool();
        // var thickness = 10f;
        //
        // var union = GeometryExt
        //     .GetCellUnionPolygons(
        //         cells, RelTo, c.Data);
        // foreach (var boundary in union)
        // {
        //     Add(boundary,
        //         () =>
        //         {
        //             var mapPos = c.Cam().GetMousePosInMapSpace();
        //             var cell = c.Data.Planet.MapAux.CellGrid.GetElementAtPoint(mapPos, c.Data);
        //             c.Notices.Selecting.Invoke(cell);
        //         }
        //     );
        // }
        // uiEls.Add(this);

        mb.DrawCellsBordersInsetLocal(cells, 
            army.Regime.Get(c.Data).PrimaryColor.Tint(.25f),
            army.Color, 
            2f, 3f, RelTo, c.Data);

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