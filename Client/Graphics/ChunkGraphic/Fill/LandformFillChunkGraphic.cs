
using Godot;

public partial class LandformFillChunkGraphic : PolyCellFillChunkGraphic
{
    public LandformFillChunkGraphic(MapChunk chunk, 
        GraphicsSegmenter segmenter, Data data) 
        : base("Landform", chunk, 
            segmenter, LayerOrder.Terrain,
            data)
    {
    }

    public override Color GetColor(Cell poly, Data d)
    {
        return poly.GetLandform(d)
            .Color
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble, TerrainChunkModule.ColorWobble));
    }

    public override bool IsValid(Cell c, Data d)
    {
        return true;
    }
}