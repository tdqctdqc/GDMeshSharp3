
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public class UnitGraphicLayer : GraphicLayer<UnitGroup, UnitGroupGraphic>
{
    public UnitGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base("Units", segmenter,
            (unit, graphic, seg, queue) => graphic.Update(unit, d, seg, queue))
    {
        this.RegisterForEntityLifetime(client, d);
    }

    protected override UnitGroupGraphic GetGraphic(UnitGroup key, Data d)
    {
        return new UnitGroupGraphic(key, _segmenter, d);
    }
}