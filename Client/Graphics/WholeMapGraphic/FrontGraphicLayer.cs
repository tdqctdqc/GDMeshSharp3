
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class FrontGraphicLayer : WholeMapGraphicLayer<Front, FrontGraphic>
{
    public FrontGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) : base("Fronts", segmenter, new List<ISettingsOption>())
    {
        this.RegisterForEntityLifetime(client, d);
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

    protected override FrontGraphic GetGraphic(Front key, Data d)
    {
        return new FrontGraphic(key, _segmenter, d);
    }
}