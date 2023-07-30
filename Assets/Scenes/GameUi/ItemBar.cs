using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ItemBar : HBoxContainer
{
    public ItemBar(Data data)
    {
        AddFlow(FlowManager.ConstructionCap, data);
        AddFlow(FlowManager.IndustrialPower, data);
        AddFlow(FlowManager.Income, data);

        foreach (var kvp in data.Models.Items.Models)
        {
            AddItem(kvp.Value, data);
        }
    }

    private ItemBar()
    {
    }

    private void AddItem(Item sr, Data data)
    {
        var hBox = sr.Icon.MakeIconStatDisplay(
            data, 
            () =>
            {
                var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data);
                return r != null ? r.Items[sr].ToString() : 0.ToString();
            }, 
            10f,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank,
            data.Notices.Ticked.Blank,
            data.Notices.FinishedTurnStartCalc
        );
        this.AddChildWithVSeparator(hBox);
    }

    private void AddFlow(Flow flow, Data data)
    {
        var conCap = NodeExt.MakeFlowStatDisplay(flow, 
            data, 
            10f,
            data.Notices.Ticked.Blank,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank
        );
        this.AddChildWithVSeparator(conCap);
    }
}