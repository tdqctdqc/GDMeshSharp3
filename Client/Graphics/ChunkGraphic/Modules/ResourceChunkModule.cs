using System;
using System.Collections.Generic;
using System.Linq;

public partial class ResourceChunkModule : MapChunkGraphicModule
{
    private ResourcePolyFill _fill;
    private ResourceIcons _icons;
    public ResourceChunkModule(MapChunk chunk, Data data) 
        : base(chunk, nameof(ResourceChunkModule))
    {
        _fill = new ResourcePolyFill(chunk, data);
        AddNode(_fill);
        _icons = new ResourceIcons(chunk, data);
        AddNode(_icons);
    }
    
    public static ChunkGraphicLayer<ResourceChunkModule> GetLayer(int z,
        Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<ResourceChunkModule>(
            z,
            "Resources",
            segmenter, 
            c => new ResourceChunkModule(c, d), 
            d);
        l.AddTransparencySetting(m => m._fill, "Transparency");
        return l;
    }
}
