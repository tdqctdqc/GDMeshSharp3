
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class FrontGraphicLayer : GraphicLayer<Front, FrontGraphic>
{
    
    private FrontGraphicLayer(Client client, GraphicsSegmenter segmenter, Data d) 
        : base("Fronts", segmenter,
            (front, graphic, seg, queue) =>
            {
                graphic.Update(front, d, seg, queue);
            })
    {
    }

    private void Draw(Data data)
    {
        foreach (var front in Graphics.Keys.ToList())
        {
            Remove(front, data);
        }
        var fronts = data.HostLogicData.RegimeAis.Dic.Values
            .SelectMany(rAi => rAi.Military.Deployment.GetFrontAssignments())
            .Select(fa => fa.Front);
        foreach (var front in fronts)
        {   
            Add(front, data);
        }
    }
    public static FrontGraphicLayer GetLayer(Client client, GraphicsSegmenter segmenter, Data d)
    {
        var l = new FrontGraphicLayer(client, segmenter, d);
        
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
            {
                client.QueuedUpdates.Enqueue(() =>
                {
                    l.Draw(d);
                    l.EnforceSettings();
                });
            }
        );
        
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
                    f.Visible = f.Front.Regime.RefId == currRegime.RefId 
                        && l.Visible;
                }
                else
                {
                    f.Visible = true && l.Visible;
                }
            });
        l.EnforceSettings();
        return l;
    }

    protected override FrontGraphic GetGraphic(Front key, Data d)
    {
        return new FrontGraphic(key, _segmenter, d);
    }
}