using System;
using System.Collections.Generic;
using System.Linq;

public partial class AllianceChunkModule : ChunkGraphicMultiModule
{
    private AlliancePolyFill _fill;
    private AllianceBordersGraphic _borders;

    public AllianceChunkModule(MapChunk chunk, GraphicsSegmenter segmenter, Data data) 
        : base()
    {
        _fill = new AlliancePolyFill(chunk, segmenter, data);
        _borders = new AllianceBordersGraphic(chunk, data, false);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _fill;
        yield return _borders;
    }
}
