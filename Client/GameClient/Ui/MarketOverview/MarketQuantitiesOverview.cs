using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MarketQuantitiesOverview : HBoxContainer
{
    private ItemSelect _itemList;
    private Control _chart;
    private Label _selectedLabel;
    public MarketQuantitiesOverview(Data data)
    {
        Name = "Quantities";
        var buffer = new Control();
        buffer.CustomMinimumSize = new Vector2(20f, 0f);
        AddChild(buffer);
        _itemList = ItemSelect.ConstructIcon<Item>(
            data.Models.GetModels<Item>().Values
                .Where(i => i is TradeableItem).ToList(), i => i.Icon.BaseTexture, 
            new Vector2I(50, 50),
            i => Draw(data), 
            i => i.Color);
        _itemList.CustomMinimumSize = new Vector2(60f, 10f);
        
        var buffer2 = new Control();
        buffer2.CustomMinimumSize = new Vector2(50f, 0f);
        AddChild(buffer2);
        
        AddChild(_itemList);
        _chart = new Control();
        AddChild(_chart);
        _selectedLabel = new Label();
        AddChild(_selectedLabel);
    }

    private MarketQuantitiesOverview()
    {
    }

    public void Draw(Data data)
    {
        _chart.ClearChildren();
        _selectedLabel.Text = "";
        var item = (Item)_itemList.Selected;
        if (item == null) return;
        _selectedLabel.Text = item.Name;
        var itemHist = data.Society.Market.TradeHistory.GetOrdered(item);
        var names = new List<string> { "Offered", "Demanded", "Exchanged"};
        var lines = new List<List<Vector2>>
        {
            itemHist.Select(info => new Vector2(info.Tick, info.TotalOffered)).ToList(),
            itemHist.Select(info => new Vector2(info.Tick, info.TotalDemanded)).ToList(),
            itemHist.Select(info => new Vector2(info.Tick, info.TotalTraded)).ToList(),
        };
        var colors = new List<Color> {Colors.Red, Colors.Blue, Colors.White};
        var chart = new LineChart(Vector2.One * 400f,
            1f,
            names,
            lines,
            colors, true, false
        );
        _chart.AddChild(chart);
    }
}
