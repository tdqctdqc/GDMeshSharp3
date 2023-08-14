using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MarketPricesOverview : HBoxContainer
{
    private ItemMultiSelect _itemList;
    private Control _chart;
    public MarketPricesOverview(Data data)
    {
        Name = "Prices";
        var buffer = new Control();
        buffer.CustomMinimumSize = new Vector2(20f, 0f);
        AddChild(buffer);
        _itemList = ItemMultiSelect.ConstructIcon<Item>(
            data.Models.GetModels<Item>().Values
                        .Where(i => i is TradeableItem).ToList(), i => i.Icon.BaseTexture, 
            new Vector2I(50, 50),
            () => Draw(data), 
            i => i.Color);
        _itemList.CustomMinimumSize = new Vector2(60f, 10f);
        
        var buffer2 = new Control();
        buffer2.CustomMinimumSize = new Vector2(50f, 0f);
        AddChild(buffer2);
        
        AddChild(_itemList);
        _chart = new Control();
        AddChild(_chart);
    }

    public void Draw(Data data)
    {
        _chart.ClearChildren();
        var items = _itemList.GetSelectedItems<Item>().Select(i => (Item) i);        
        var chart = new LineChart(Vector2.One * 400f,
            1f,
            items.Select(i => i.Name).ToList(),
            items.Select(i =>
            {
                return data.Society.Market.TradeHistory.GetOrdered(i)
                    .Select(r => new Vector2(r.Tick, r.Price)).ToList();
            }).ToList(),
            items.Select(i => i.Color).ToList(), true, false
        );
        _chart.AddChild(chart);
    }
    
}
