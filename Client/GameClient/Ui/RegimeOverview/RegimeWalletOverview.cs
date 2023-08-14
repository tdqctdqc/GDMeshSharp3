using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeWalletOverview : ScrollContainer
{
    private VBoxContainer _container;
    public RegimeWalletOverview()
    {
        Name = "Wallet";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var itemIds = data.Models.GetModels<Item>().Values;
        var tick = data.BaseDomain.GameClock.Tick;
        
        foreach (var item in itemIds)
        {
            var amt = regime.Items[item];
            var itemReport = regime.History.ItemHistory.Latest(item);
            
            var hbox = new HBoxContainer();
            
            hbox.AddChild(item.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"Amount: {amt} ");
            hbox.CreateLabelAsChild($"Prod: {itemReport.Produced} ");
            hbox.CreateLabelAsChild($"Consumed: {itemReport.Consumed} ");
            if (item is TradeableItem)
            {
                hbox.CreateLabelAsChild($"Bought: {itemReport.Bought} ");
                hbox.CreateLabelAsChild($"Sold: {itemReport.Sold} ");
                hbox.CreateLabelAsChild($"Offered: {itemReport.Offered} ");
                hbox.CreateLabelAsChild($"Demanded: {itemReport.Demanded} ");
            }
            _container.AddChild(hbox);
        }
    }
}
