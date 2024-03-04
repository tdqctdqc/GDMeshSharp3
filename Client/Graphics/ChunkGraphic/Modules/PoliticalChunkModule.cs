
using System.Collections.Generic;

public partial class PoliticalChunkModule 
    : ChunkGraphicMultiModule
{
    private PoliticalFillModule _fill;
    private PoliticalBordersModule _border;
    public enum Mode
    {
        Regime, Alliance, Diplomacy
    }
    public Mode SelectedMode { get; private set; }
    public PoliticalChunkModule(MapChunk chunk, 
        Data d)
    {
        SelectedMode = Mode.Regime;
        _fill = new PoliticalFillModule(this, chunk, d);
        _border = new PoliticalBordersModule(this, chunk, d);
        foreach (var m in GetModules())
        {
            AddChild(m.Node);
        }
    }

    protected override IEnumerable<IChunkGraphicModule> GetModules()
    {
        yield return _fill;
        yield return _border;
    }
}