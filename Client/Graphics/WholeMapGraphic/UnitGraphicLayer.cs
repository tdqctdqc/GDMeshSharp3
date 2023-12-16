
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public class UnitGraphicLayer : GraphicLayer<UnitGroup, UnitGroupGraphic>
{
    public UnitGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base(LayerOrder.Units, "Units", segmenter)
    {
        this.RegisterForEntityLifetime(client, d);
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
        {
            foreach (var g in Graphics)
            {
                g.Value.Update(g.Key, d, segmenter, client.QueuedUpdates);
            }
        });
    }

    protected override UnitGroupGraphic GetGraphic(UnitGroup key, Data d)
    {
        return new UnitGroupGraphic(key, _segmenter, d);
    }
}