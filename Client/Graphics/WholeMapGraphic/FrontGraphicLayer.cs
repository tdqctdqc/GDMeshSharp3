
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class FrontGraphicLayer : GraphicLayer<Front, FrontGraphic>
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
            (f, v) => f.FrontNode.Visible = v);
        l.AddSetting(new BoolSettingsOption("Show Line", true),
            (f, v) => f.LineNode.Visible = v);
        
        l.AddSetting(new BoolSettingsOption("Only show for current regime",
            false), (f, onlyShowCurr) =>
            {
                var currRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime;
                if (onlyShowCurr)
                {
                    f.Visible = f.Front.Regime.RefId == currRegime.RefId;
                }
                else
                {
                    f.Visible = true;
                }
            });
        return l;
    }

    protected override FrontGraphic GetGraphic(Front key, Data d)
    {
        return new FrontGraphic(key, _segmenter, d);
    }
}