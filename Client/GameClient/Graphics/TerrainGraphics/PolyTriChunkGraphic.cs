using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public partial class PolyTriChunkGraphic : MapChunkGraphicModule
{
    public PolyTriChunkGraphic(MapChunk chunk, Data data) : base(nameof(PolyTriChunkGraphic))
    {
        var lfLayer = new PolyTriLayer("Landform", data, t => t.Landform.Color, 
            new Vector2(0f, 1f), chunk, null);
        AddLayer(lfLayer);
        var vegLayer = new PolyTriLayer("Vegetation", data, t => t.Vegetation.Color.Darkened(t.Landform.DarkenFactor), 
            new Vector2(0f, 1f), chunk, null);
        AddLayer(vegLayer);
    }

    private partial class PolyTriLayer : MapChunkGraphicLayer<Vector2>
    {
        private Func<PolyTri, Color> _getColor;
        public PolyTriLayer(string name, Data data, Func<PolyTri, Color> getColor, 
            Vector2 visRange, MapChunk chunk,
            ChunkChangeListener<Vector2> listener) 
            : base(name, data, chunk, visRange, listener)
        {
            _getColor = getColor;
        }
        protected override Node2D MakeGraphic(Vector2 key, Data data)
        {
            var mb = new MeshBuilder();
            foreach (var p in Chunk.Polys)
            {
                var offset = Chunk.RelTo.GetOffsetTo(p, data);
                foreach (var tri in p.Tris.Tris)
                {
                    mb.AddTri(tri.Transpose(offset), _getColor(tri));
                }
            }

            return mb.GetMeshInstance();
        }

        protected override IEnumerable<Vector2> GetKeys(Data data)
        {
            return new List<Vector2>{Chunk.Coords};
        }
    }
}