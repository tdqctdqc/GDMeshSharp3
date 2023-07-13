using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public partial class PolyTriChunkGraphic : MapChunkGraphicModule
{
    public PolyTriChunkGraphic(MapChunk chunk, Data data, MapGraphics mg)
    {
        var lfLayer = new PolyTriLayer(data, t => t.Landform.Color, 
            chunk, null);
        AddLayer(new Vector2(0f, 1f), lfLayer);
        var vegLayer = new PolyTriLayer(data, t => t.Vegetation.Color.Darkened(t.Landform.DarkenFactor), 
            chunk, null);
        AddLayer(new Vector2(0f, 1f), vegLayer);
    }

    private PolyTriChunkGraphic()
    {
        
    }

    private partial class PolyTriLayer : MapChunkGraphicLayer<Vector2>
    {
        private Func<PolyTri, Color> _getColor;
        public PolyTriLayer(Data data, Func<PolyTri, Color> getColor, MapChunk chunk, ChunkChangeListener<Vector2> listener) 
            : base(data, chunk, listener)
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