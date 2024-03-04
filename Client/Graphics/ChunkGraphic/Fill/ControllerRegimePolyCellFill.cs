
using System;
using Godot;

public partial class ControllerRegimePolyCellFill 
    : PolyCellFillChunkGraphic
{
    public ControllerRegimePolyCellFill(MapChunk chunk,
        Data data) 
        : base("Controller", chunk,
            LayerOrder.PolyFill, data)
    {
    }

    public override Color GetColor(Cell cell, Data d)
    {
        var r = cell.Controller.Entity(d);
        if(r == null) return Colors.Transparent;
        return cell.Controller.Entity(d).GetMapColor();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }
}