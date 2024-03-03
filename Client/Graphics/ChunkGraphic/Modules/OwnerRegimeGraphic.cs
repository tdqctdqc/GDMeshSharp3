using System;
using System.Collections.Generic;
using System.Linq;

public partial class OwnerRegimeGraphic : ChunkGraphicMultiModule
{
    private RegimePolyFill _fill;
    private RegimeBordersGraphic _borders;
    public OwnerRegimeGraphic(MapChunk chunk,
        GraphicsSegmenter segmenter, 
        Data data) 
    {
        _fill = new RegimePolyFill(chunk, segmenter, data);
        _borders = new RegimeBordersGraphic(chunk, data);
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _fill;
        yield return _borders;
    }

}
