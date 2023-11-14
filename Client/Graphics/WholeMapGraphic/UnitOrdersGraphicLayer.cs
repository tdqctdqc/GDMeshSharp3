
using System;
using System.Collections.Concurrent;
using Godot;

public class UnitOrdersGraphicLayer : GraphicLayer<UnitGroup, UnitOrderGraphic>
{
    public UnitOrdersGraphicLayer(string name, Client client,
        GraphicsSegmenter segmenter) 
        : base(name, segmenter,
            (group, graphic, seg, update) =>
            {
                graphic.Update(client.Data, seg, client.QueuedUpdates);
            })
    {
    }
    
    protected override UnitOrderGraphic GetGraphic(UnitGroup key, Data d)
    {
        return new UnitOrderGraphic(key, _segmenter, d);
    }

    public static UnitOrdersGraphicLayer GetLayer(GraphicsSegmenter segmenter,
        Client client)
    {
        var l = new UnitOrdersGraphicLayer("Unit Orders", client,
            segmenter);
        l.RegisterForEntityLifetime(client, client.Data);
        client.Data.Notices.Ticked.Blank.Subscribe(
            () =>
            {
                client.QueuedUpdates.Enqueue(() => 
                    l.Update(client.Data, client.QueuedUpdates));
            }
        );
        l.AddSetting(new BoolSettingsOption("Only show for current regime",
            false), (unitOrderGraphic, onlyShowCurr) =>
        {
            var currRegime = client.Data.BaseDomain.PlayerAux.LocalPlayer.Regime;
            if (onlyShowCurr)
            {
                unitOrderGraphic.Visible = unitOrderGraphic.Group.Regime.RefId == currRegime.RefId;
            }
            else
            {
                unitOrderGraphic.Visible = true;
            }
        });
        return l;
    }
}