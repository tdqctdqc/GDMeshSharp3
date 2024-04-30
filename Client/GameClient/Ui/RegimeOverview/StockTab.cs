using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class StockTab : ScrollContainer
{
    private VBoxContainer _container;
    public StockTab()
    {
        Name = "Stock";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        var tick = client.Data.BaseDomain.GameClock.Tick;
        var iconSize = client.Settings.MedIconSize.Value;

        foreach (var entry in regime.Stock.Stock.GetEnumerableModel(client.Data))
        {
            var model = entry.Key;
            var amt = entry.Value;
            var hbox = new HBoxContainer();

            if (model is IIconed i)
            {
                hbox.AddChild(i.Icon.GetTextureRect(iconSize));
            }
            else
            {
                hbox.CreateLabelAsChild(model.Name);
            }
            
            hbox.CreateLabelAsChild($"Amount: {amt} ");
            hbox.CreateLabelAsChild($"Prod: {regime.Stock.Produced.Get(model)} ");
            hbox.CreateLabelAsChild($"Single time: {regime.Stock.SingleTimeCosts.Get(model)} ");
            hbox.CreateLabelAsChild($"Recurring: {regime.Stock.RecurringCosts.Get(model)} ");
            
            _container.AddChild(hbox);
        }
    }
}
