using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SettlementIcons : MapChunkGraphicNode<Settlement>
{
    public SettlementIcons(MapChunk chunk, Data data) 
        : base(nameof(SettlementIcons), data, chunk)
    {
    }

    private SettlementIcons() : base()
    {
    }


    protected override Node2D MakeGraphic(Settlement element, Data data)
    {
        var node = new Node2D();
        var icon = element.Tier.Model(data).Icon;
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var poly = element.Poly.Entity(data);
        var urbanCells = poly.GetCells(data)
            .Where(t => t.GetLandform(data) == data.Models.Landforms.Urban);
        if (urbanCells.Count() == 0)
        {
            GD.Print("no urban tri settlement at " + element.Poly.Entity(data).Id);
            return new Node2D();
        }
        foreach (var urbanCell in urbanCells)
        {
            var mesh = icon.GetMeshInstance(size);
            SetRelPos(mesh, urbanCell.GetCenter(), data);
            node.AddChild(mesh);
        }

        var nameNode = new Node2D();
        var nameLabel = new Label();
        nameLabel.Text = element.Name;
        nameLabel.Theme = UiThemes.DefaultTheme;
        nameLabel.LabelSettings = UiThemes.MapLabelSettings;
        SetRelPos(nameNode, urbanCells.First().GetCenter(), data);
        nameNode.AddChild(nameLabel);
        node.AddChild(nameNode);

        return node;
    }

    protected override IEnumerable<Settlement> GetKeys(Data data)
    {
        return Chunk.Polys.Where(p => p.HasSettlement(data))
            .Select(p => p.GetSettlement(data));
    }

    protected override bool Ignore(Settlement element, Data data)
    {
        return false;
    }
}

