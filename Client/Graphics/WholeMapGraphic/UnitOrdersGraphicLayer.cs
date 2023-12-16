
using System;
using System.Collections.Concurrent;
using Godot;

public class UnitOrdersGraphicLayer : GraphicLayer<UnitGroup, UnitOrderGraphic>
{
    public UnitOrdersGraphicLayer(LayerOrder z, string name, Client client,
        GraphicsSegmenter segmenter) 
        : base(z, name, segmenter)
    {
    }
    
    protected override UnitOrderGraphic GetGraphic(UnitGroup key, Data d)
    {
        return new UnitOrderGraphic(key, _segmenter, d);
    }

    public static UnitOrdersGraphicLayer GetLayer(GraphicsSegmenter segmenter,
        Client client)
    {
        var l = new UnitOrdersGraphicLayer(LayerOrder.UnitOrders, "Unit Orders", client,
            segmenter);
        l.RegisterForEntityLifetime(client, client.Data);
        client.Data.Notices.Ticked.Blank.Subscribe(
            () =>
            {
                foreach (var kvp in l.Graphics)
                {
                    kvp.Value.Update(client.Data, segmenter, client.QueuedUpdates);
                }
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