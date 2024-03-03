
using System.Collections.Generic;

public partial class ControllerRegimeGraphic : ChunkGraphicMultiModule
{
    private ControllerRegimePolyCellFill _fill;
    private ControllerPolyCellBorder _borders;
    public ControllerRegimeGraphic(MapChunk chunk, 
        GraphicsSegmenter segmenter, Data data) 
        : base()
    {
        _fill = new ControllerRegimePolyCellFill(chunk, segmenter, data);
        _borders = new ControllerPolyCellBorder(chunk, segmenter, data);
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