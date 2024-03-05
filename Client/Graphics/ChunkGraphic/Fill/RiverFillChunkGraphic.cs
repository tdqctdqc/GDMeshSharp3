
using Godot;

public partial class RiverFillChunkGraphic 
    : PolyCellFillChunkGraphic
{
    private static Color _riverColor;
    public RiverFillChunkGraphic(MapChunk chunk,
        Data data) 
        : base("River", chunk, 
            LayerOrder.Rivers, data)
    {
        _riverColor = data.Models.Landforms.River
            .Color;
    }

    public override Color GetColor(Cell cell, Data d)
    {
        if(cell is RiverCell)
        {
            return _riverColor
                .Darkened(Game.I.Random
                    .RandfRange(-TerrainChunkModule.ColorWobble,
                        TerrainChunkModule.ColorWobble));
        }
        return Colors.Transparent;
    }

    public override void RegisterForRedraws(Data d)
    {
        
    }

    public override void DoUiTick(UiTickContext context, Data d)
    {
        
    }
}