
using System;
using Godot;

public partial class PaintPolyCellChunkGraphic : PolyCellFillChunkGraphic
{
    private Func<Cell, Color> _getColor;
    public PaintPolyCellChunkGraphic(string name, 
        Func<Cell, Color> getColor,
        MapChunk chunk, LayerOrder layerOrder, Vector2 zoomVisRange, Data data) : base(name, chunk, layerOrder, zoomVisRange, data)
    {
        _getColor = getColor;
    }

    public override Color GetColor(Cell cell, Data d)
    {
        return _getColor(cell);
    }

    public override void RegisterForRedraws(Data d)
    {
        
    }

    public override Settings GetSettings(Data d)
    {
        var s = new Settings("doot");
        s.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        return s;
    }
}