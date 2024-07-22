using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class StockTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public StockTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Stock";

        CustomMinimumSize = new Vector2(200f, 400f);
        _container = new VBoxContainer();
        _container.CustomMinimumSize = CustomMinimumSize;
        AddChild(_container);
    }
    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
        var tick = client.Data.BaseDomain.GameClock.Tick;
        var iconSize = client.Settings.MedIconSize.Value;
        var items = regime.Stock.Stock.GetEnumModel(client.Data)
            .Where(kvp => kvp.Key is not Flow
                          && kvp.Key is Item);
        
        var flows = regime.Stock.Stock.GetEnumModel(client.Data)
            .Where(kvp => kvp.Key is Flow);
        
        var rest = regime.Stock.Stock.GetEnumModel(client.Data)
            .Where(kvp => kvp.Key is not Flow
                && kvp.Key is not Item);
        
        _container.CreateLabelAsChild("Flows");
        
        foreach (var entry in flows)
        {
            makeEntry(entry);
        }

        _container.CreateLabelAsChild("Items");
        
        foreach (var entry in items)
        {
            makeEntry(entry);
        }

        _container.CreateLabelAsChild("Other Models");
        foreach (var entry in rest)
        {
            makeEntry(entry);
        }

        void makeEntry(KeyValuePair<IModel, float> entry)
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
            
            hbox.CreateLabelAsChild($"Amount: {amt.RoundTo2Digits()} ");
            hbox.CreateLabelAsChild($"Prod: {regime.Stock.Produced.Get(model).RoundTo2Digits()} ");
            hbox.CreateLabelAsChild($"Single time: {regime.Stock.SingleTimeCosts.Get(model).RoundTo2Digits()} ");
            hbox.CreateLabelAsChild($"Recurring: {regime.Stock.RecurringCosts.Get(model).RoundTo2Digits()} ");
            
            _container.AddChild(hbox);
        }
    }
}
