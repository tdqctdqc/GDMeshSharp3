using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PolyFillLayer : MapChunkGraphicLayer<Vector2>
{
    private Func<MapPolygon, Color> _getColor;
    private float _transparency;
    public PolyFillLayer(string name, MapChunk chunk, Data data, Func<MapPolygon, Color> getColor, Vector2 visRange,
        float transparency = 1f) : base(name, data, chunk, visRange, null)
    {
        _transparency = transparency;
        _getColor = getColor;
    }

    private PolyFillLayer()
    {
    }
    protected override Node2D MakeGraphic(Vector2 key, Data data)
    {
        var mb = new MeshBuilder();
        mb.AddPolysRelative(Chunk.RelTo, Chunk.Polys, _getColor, data);
        var mesh = mb.GetMeshInstance();
        mesh.Modulate = new Color(Colors.Transparent, _transparency);
        return mesh;
    }

    protected override IEnumerable<Vector2> GetKeys(Data data)
    {
        return new []{Chunk.Coords};
    }
}
