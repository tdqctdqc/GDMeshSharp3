using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeStockBar : HBoxContainer
{
    public RegimeStockBar(Client client, Data data)
    {
        AddModel(client, data.Models.Flows.ConstructionCap, data);
        AddModel(client, data.Models.Flows.IndustrialPower, data);
        AddModel(client, data.Models.Flows.Income, data);
        AddModel(client, data.Models.Flows.MilitaryCap, data);
        AddModel(client, data.Models.Flows.Labor, data);

        foreach (var kvp in data.Models.GetModels<Item>())
        {
            AddModel(client, kvp.Value, data);
        }
    }

    private RegimeStockBar()
    {
    }

    private void AddModel(Client client, IModel m, Data data)
    {
        Control hBox;
        if (m is IIconed i)
        {
            hBox = i.Icon.MakeIconStatDisplay(
                client,
                data, 
                () =>
                {
                    var r = client.GetComponent<MapGraphics>().SpectatingRegime;
                    if (r == null) return "";
                    var stock = r.Stock.Stock.Get(m);
                    var recurring = r.Stock.RecurringCosts.Get(m);
                    var oneTime = r.Stock.SingleTimeCosts.Get(m);
                    var produced = r.Stock.Produced.Get(m);
                    return $"{stock} / {produced} / {recurring} / {oneTime}";
                }, 
                10f,
                Game.I.Client.Notices.ChangedSpectatingRegime.Blank,
                data.Notices.Ticked.Blank,
                data.Notices.FinishedTurnStartCalc
            );
        }
        else
        {
            hBox = NodeExt.MakeStatDisplay(
                client,
                data,
                () =>
                {
                    var r = client.GetComponent<MapGraphics>().SpectatingRegime;

                    var stock = r.Stock.Stock.Get(m);

                    return r != null ? stock.ToString() : 0.ToString();
                },
                10f,
                Game.I.Client.Notices.ChangedSpectatingRegime.Blank,
                data.Notices.Ticked.Blank,
                data.Notices.FinishedTurnStartCalc);
        }
        
        this.AddChildWithVSeparator(hBox);
    }
}