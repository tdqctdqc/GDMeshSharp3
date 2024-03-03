
using System;
using Godot;

public partial class ControllerRegimePolyCellFill 
    : PolyCellFillChunkGraphic
{
    public ControllerRegimePolyCellFill(MapChunk chunk,
        GraphicsSegmenter segmenter,
        Data data) 
        : base("Controller", chunk,
            segmenter, LayerOrder.PolyFill, data)
    {
    }

    public override Color GetColor(Cell c, Data d)
    {
        var r = c.Controller.Entity(d);
        if(r == null) return Colors.Transparent;
        return c.Controller.Entity(d).GetMapColor();
    }

    public override bool IsValid(Cell c, Data d)
    {
        return c.Controller.Fulfilled();
    }
}