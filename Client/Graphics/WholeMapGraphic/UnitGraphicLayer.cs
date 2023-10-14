
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;

public class UnitGraphicLayer : WholeMapGraphicLayer<Unit, UnitGraphic>
{
    public UnitGraphicLayer(GraphicsSegmenter segmenter, Data d) 
        : base("Units", segmenter, new List<ISettingsOption>())
    {
        this.RegisterForEntityLifetime(d);
    }

    public override void Update(Data d, ConcurrentQueue<Action> queue)
    {
        foreach (var kvp in Graphics)
        {
            var unit = kvp.Key;
            var graphic = kvp.Value;
            graphic.Update(unit, d, _segmenter, queue);
        }
    }

    protected override UnitGraphic GetGraphic(Unit key, Data d)
    {
        return UnitGraphic.Construct(key, _segmenter, d);
    }
}