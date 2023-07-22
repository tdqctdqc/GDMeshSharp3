using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SettlementIconLayer : MapChunkGraphicLayer<int>
{
    public SettlementIconLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(nameof(SettlementIconLayer), data, chunk, new Vector2(0f, .5f), mg.ChunkChangedCache.SettlementTierChanged)
    {
    }

    private SettlementIconLayer() : base()
    {
    }


    protected override Node2D MakeGraphic(int key, Data data)
    {
        var node = new Node2D();
        var settlement = data.Infrastructure.Settlements[key];
        var icon = settlement.Tier.Model(data).Icon;
        var poly = settlement.Poly.Entity(data);
        var urbanTris = poly.Tris.Tris
            .Where(t => t.Landform == LandformManager.Urban);
        foreach (var urbanTri in urbanTris)
        {
            var mesh = icon.GetMeshInstance();
            SetRelPos(mesh, new PolyTriPosition(poly.Id, urbanTri.Index), data);
            node.AddChild(mesh);
        }

        var nameNode = new Node2D();
        var nameLabel = new Label();
        nameLabel.Text = settlement.Name;
        nameLabel.Theme = UiThemes.DefaultTheme;
        nameLabel.LabelSettings = UiThemes.MapLabelSettings;
        SetRelPos(nameNode, urbanTris.First().GetPosition(), data);
        nameNode.AddChild(nameLabel);
        node.AddChild(nameNode);

        return node;
    }

    protected override IEnumerable<int> GetKeys(Data data)
    {
        return Chunk.Polys.Where(p => p.HasSettlement(data))
            .Select(p => p.GetSettlement(data).Id);
    }
}

