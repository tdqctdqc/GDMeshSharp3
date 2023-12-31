using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ItemBar : HBoxContainer
{
    public ItemBar(Client client, Data data)
    {
        AddFlow(client, data.Models.Flows.ConstructionCap, data);
        AddFlow(client, data.Models.Flows.IndustrialPower, data);
        AddFlow(client, data.Models.Flows.Income, data);

        foreach (var kvp in data.Models.GetModels<Item>())
        {
            AddItem(client, kvp.Value, data);
        }
    }

    private ItemBar()
    {
    }

    private void AddItem(Client client, Item sr, Data data)
    {
        var hBox = sr.Icon.MakeIconStatDisplay(
            client,
            data, 
            () =>
            {
                var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data);
                return r != null ? r.Items.Get(sr).ToString() : 0.ToString();
            }, 
            10f,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank,
            data.Notices.Ticked.Blank,
            data.Notices.FinishedTurnStartCalc
        );
        this.AddChildWithVSeparator(hBox);
    }

    private void AddFlow(Client client, Flow flow, Data data)
    {
        var conCap = NodeExt.MakeFlowStatDisplay(client, flow, 
            data, 
            10f,
            data.Notices.Ticked.Blank,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
        );
        this.AddChildWithVSeparator(conCap);
    }
}