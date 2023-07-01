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
        var itemIds = data.Models.Items.Models.Values.Select(i => i.Id);
        
        foreach (var itemName in itemIds)
        {
            var amt = regime.Items[itemName];
            var lastProd = regime.History.ProdHistory[itemName].GetLatest();
            var lastDemand = regime.History.DemandHistory[itemName].GetLatest();
            if (lastDemand == 0 && lastProd == 0) continue;
            
            var hbox = new HBoxContainer();
            var item = (Item)data.Models[itemName];
            
            hbox.AddChild(item.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild($"Amount: {amt} ");
            hbox.CreateLabelAsChild($"Prod: {lastProd} ");
            hbox.CreateLabelAsChild($"Demand: {lastDemand}");
            _container.AddChild(hbox);
        }
    }
}
