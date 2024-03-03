
using System.Collections.Generic;

public partial class PoliticalFillChunkModule 
    : ChunkGraphicMultiModule
{
    public OwnerRegimeGraphic Owner { get; private set; }
    public AllianceChunkModule Alliance { get; private set; }
    public DiplomacyChunkModule Diplomacy { get; private set; }

    public PoliticalFillChunkModule(MapChunk chunk, 
        GraphicsSegmenter segmenter,
        Data d)
    {
        Owner = new OwnerRegimeGraphic(chunk, segmenter, d);
        Alliance = new AllianceChunkModule(chunk, segmenter, d);
        Diplomacy = new DiplomacyChunkModule(chunk, segmenter, d);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return Owner;
        yield return Alliance;
        yield return Diplomacy;
    }
}