using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class ItemsTab : ScrollContainer
{
    private VBoxContainer _container;
    public ItemsTab()
    {
        Name = "Items";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        var itemIds = client.Data.Models.GetModels<Item>().Values;
        var tick = client.Data.BaseDomain.GameClock.Tick;
        var iconSize = client.Settings.MedIconSize.Value;

        foreach (var item in itemIds)
        {
            var amt = regime.Items.Get(item);
            var itemReport = regime.History.ItemHistory.GetLatest(item);
            
            var hbox = new HBoxContainer();
            
            hbox.AddChild(item.Icon.GetTextureRect(iconSize));
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
