using System;
using System.Collections.Generic;
using System.Linq;

public partial class ResourceChunkModule : ChunkGraphicMultiModule
{
    private ResourcePolyFill _fill;
    private ResourceIcons _icons;
    public ResourceChunkModule(MapChunk chunk, 
        GraphicsSegmenter segmenter,
        Data data) 
    {
        _fill = new ResourcePolyFill(chunk, segmenter, data);
        _icons = new ResourceIcons(chunk, data);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _fill;
        yield return _icons;
    }
}
