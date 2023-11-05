
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class FrontGraphicLayer : WholeMapGraphicLayer<Front, FrontGraphic>
{
    
    private FrontGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base("Fronts", segmenter, 
            (front, graphic, seg, queue) => graphic.Update(front, d, seg, queue))
    {
    }
    public static FrontGraphicLayer GetLayer(Client client, GraphicsSegmenter segmenter, Data d)
    {
        var l = new FrontGraphicLayer(client, segmenter, d);
        l.RegisterForEntityLifetime(client, d);
        l.AddSetting(new BoolSettingsOption("Show Area", true),
            (f, v) => f.Front.Visible = v);
        l.AddSetting(new BoolSettingsOption("Show Line", true),
            (f, v) => f.Line.Visible = v);
        return l;
    }

    protected override FrontGraphic GetGraphic(Front key, Data d)
    {
        return new FrontGraphic(key, _segmenter, d);
    }
}