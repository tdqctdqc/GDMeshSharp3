using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DiplomacyChunkModule : ChunkGraphicMultiModule
{
    private DiplomacyPolyFill _diplomacyFill;
    private AllianceBordersGraphic _allianceBorder;
    
    public DiplomacyChunkModule(MapChunk chunk, 
        GraphicsSegmenter segmenter,
        Data data) 
    {
        _diplomacyFill = new DiplomacyPolyFill(chunk, 
            segmenter, data);
        _allianceBorder = new AllianceBordersGraphic(chunk, data, true);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _diplomacyFill;
        yield return _allianceBorder;
    }
}
