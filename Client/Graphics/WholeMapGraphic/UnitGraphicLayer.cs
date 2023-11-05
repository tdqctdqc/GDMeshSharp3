
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public class UnitGraphicLayer : GraphicLayer<Unit, UnitGraphic>
{
    public UnitGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base("Units", segmenter,
            (unit, graphic, seg, queue) => graphic.Update(unit, d, seg, queue))
    {
        this.RegisterForEntityLifetime(client, d);
    }

    protected override UnitGraphic GetGraphic(Unit key, Data d)
    {
        return new UnitGraphic(key, _segmenter, d);
    }
}