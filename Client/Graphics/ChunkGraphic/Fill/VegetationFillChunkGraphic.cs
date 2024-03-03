
using Godot;

public partial class VegetationFillChunkGraphic 
    : PolyCellFillChunkGraphic
{
    public VegetationFillChunkGraphic(MapChunk chunk,
        GraphicsSegmenter segmenter, 
        Data data) 
        : base("Vegetation", chunk, 
            segmenter, LayerOrder.Terrain,
            data)
    {
    }

    public override Color GetColor(Cell pt, Data d)
    {
        return pt.GetVegetation(d)
            .Color.Darkened(pt.GetLandform(d).DarkenFactor)
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble,
                TerrainChunkModule.ColorWobble));
    }

    public override bool IsValid(Cell c, Data d)
    {
        return c is LandCell;
    }
}