
using Godot;

public partial class RiverFillChunkGraphic 
    : PolyCellFillChunkGraphic
{
    public RiverFillChunkGraphic(MapChunk chunk,
        GraphicsSegmenter segmenter,
        Data data) 
        : base("River", chunk, 
            segmenter, LayerOrder.Rivers, data)
    {
    }

    public override Color GetColor(Cell poly, Data d)
    {
        return d.Models.Landforms.River
            .Color
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble, TerrainChunkModule.ColorWobble));

    }

    public override bool IsValid(Cell c, Data d)
    {
        return c is RiverCell;
    }
}