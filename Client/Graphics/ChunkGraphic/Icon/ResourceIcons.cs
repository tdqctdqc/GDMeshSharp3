using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ResourceIcons : MapChunkGraphicNode<MapPolygon>
{
    public ResourceIcons(MapChunk chunk, Data data) 
        : base(nameof(ResourceIcons), data, chunk)
    {
    }
    private ResourceIcons() : base()
    {
    }

    protected override Node2D MakeGraphic(MapPolygon element, Data data)
    {
        var deps = element.GetResourceDeposits(data);
        var node = new Node2D();
        var hbox = new HBoxContainer();
        hbox.Alignment = BoxContainer.AlignmentMode.Center;
        node.AddChild(hbox);
        var dim = 40f;
        foreach (var dep in deps)
        {
            if (dep.Size == 0) continue;
            var icon = dep.Item.Model(data).Icon.GetTextureRect(Vector2.One * dim);
            hbox.AddChild(icon);
        }

        hbox.Position = new Vector2(-dim * deps.Count() / 2f, 0f);
        SetRelPos(node, element, data);
        return node;
    }

    protected override IEnumerable<MapPolygon> GetKeys(Data data)
    {
        return Chunk.Polys;
    }

    protected override bool Ignore(MapPolygon element, Data data)
    {
        return element.GetResourceDeposits(data) == null;
    }
}
