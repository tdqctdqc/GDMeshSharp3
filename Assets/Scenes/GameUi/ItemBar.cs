using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ItemBar : HBoxContainer
{
    public void Setup(Data data)
    {
        foreach (var kvp in data.Models.Items.Models)
        {
            AddItem(kvp.Value, data);
        }
    }
    private void AddItem(Item sr, Data data)
    {
        var hBox = sr.Icon.MakeIconStatDisplay(
            data, 
            () =>
            {
                var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity();
                return r != null ? r.Items[sr].ToString() : 0.ToString();
            }, 
            20f,
            data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank,
            data.Notices.Ticked.Blank
        );
        this.AddChildWithVSeparator(hBox);
    }
}