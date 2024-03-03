using System.Collections.Generic;
using Godot;

public partial class ChunkGraphicHolder : Node2D
{
    public MapChunk Chunk { get; private set; }
    public List<IChunkGraphicModule> Graphics { get; private set; }
    private GraphicsSegmenter _segmenter;

    public ChunkGraphicHolder(MapChunk chunk, 
        GraphicsSegmenter segmenter, Data d)
    {
        Chunk = chunk;
        _segmenter = segmenter;
        Graphics = GetModules(segmenter, d);
        Graphics.ForEach(g =>
        {
            AddChild(g.Node);
            g.Draw(d);
        });
        _segmenter.AddElement(this, chunk.RelTo.Center);
    }

    private List<IChunkGraphicModule> GetModules(GraphicsSegmenter segmenter, 
        Data d)
    {
        return new List<IChunkGraphicModule>()
        {
            new TerrainChunkModule(Chunk, segmenter, d),
            new PoliticalFillChunkModule(Chunk, segmenter, d),
            new RoadChunkGraphicNode(Chunk, d),
            new IconsChunkModule(Chunk, d),
            new ResourceChunkModule(Chunk, segmenter, d),
        };
    }
}