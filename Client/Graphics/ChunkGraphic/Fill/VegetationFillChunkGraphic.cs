
using Godot;

public partial class VegetationFillChunkGraphic 
    : PolyCellFillChunkGraphic
{
    public VegetationFillChunkGraphic(MapChunk chunk,
        Data data) 
        : base("Vegetation", chunk, 
            LayerOrder.Terrain,
            data)
    {
    }

    public override Color GetColor(Cell cell, Data d)
    {
        return cell.GetVegetation(d)
            .Color.Darkened(cell.GetLandform(d).DarkenFactor)
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble, TerrainChunkModule.ColorWobble))
            ;
    }

    public override void RegisterForRedraws(Data d)
    {
        
    }
}