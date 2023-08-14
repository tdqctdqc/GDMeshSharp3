using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MarketOverviewWindow : TabWindow
{
    public MarketOverviewWindow(Data data)
    {
        var prices = new MarketPricesOverview(data);
        AddTab(prices);
        var qs = new MarketQuantitiesOverview(data);
        AddTab(qs);
        AboutToPopup += () =>
        {
            prices.Draw(data);
            qs.Draw(data);
        };
        MinSize = new Vector2I(700, 500);
    }

    private MarketOverviewWindow()
    {
    }
}
