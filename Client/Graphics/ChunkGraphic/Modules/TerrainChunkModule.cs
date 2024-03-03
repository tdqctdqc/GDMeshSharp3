using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TerrainChunkModule : ChunkGraphicMultiModule
{
    public static float ColorWobble = .05f;

    private LandformFillChunkGraphic _lf;
    private VegetationFillChunkGraphic _v;
    private RiverFillChunkGraphic _r;
    
    public TerrainChunkModule(MapChunk chunk, 
        GraphicsSegmenter segmenter, Data data)
    {
        _lf = new LandformFillChunkGraphic(chunk, segmenter, data);
        _v = new VegetationFillChunkGraphic(chunk, segmenter, data);
        _r = new RiverFillChunkGraphic(chunk, segmenter, data);
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _lf;
        yield return _v;
        yield return _r;
    }
}
