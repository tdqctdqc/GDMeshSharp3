
using Godot;

public partial class LandformFillChunkGraphic : PolyCellFillChunkGraphic
{
    public LandformFillChunkGraphic(MapChunk chunk, 
        Data data) 
        : base("Landform", chunk, 
            LayerOrder.Terrain,
            data)
    {
    }

    public override Color GetColor(Cell cell, Data d)
    {
        return cell.GetLandform(d)
            .Color
            .Darkened(Game.I.Random.RandfRange(-TerrainChunkModule.ColorWobble, TerrainChunkModule.ColorWobble));
    }

    public override void RegisterForRedraws(Data d)
    {
        
    }

    public override void DoUiTick(UiTickContext context, Data d)
    {
        
    }
}