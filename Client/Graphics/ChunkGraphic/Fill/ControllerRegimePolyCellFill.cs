
using System;
using System.Collections.Generic;
using Godot;

public partial class ControllerRegimePolyCellFill 
    : PolyCellFillChunkGraphic
{
    public ControllerRegimePolyCellFill(MapChunk chunk,
        Data data) 
        : base("Controller", chunk,
            LayerOrder.PolyFill,
            new Vector2(0f, 1f), data)
    {
    }

    public override Color GetColor(Cell cell, Data d)
    {
        var r = cell.Controller.Get(d);
        if(r == null) return Colors.Transparent;
        return cell.Controller.Get(d).GetMapColor();
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }

    public override Settings GetSettings(Data d)
    {
        var settings = new Settings(Name);
        settings.SettingsOptions.Add(
            this.MakeVisibilitySetting(true));
        settings.SettingsOptions.Add(
            this.MakeTransparencySetting());
        
        return settings;
    }
}