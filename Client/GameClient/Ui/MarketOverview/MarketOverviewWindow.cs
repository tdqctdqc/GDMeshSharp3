using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class MarketOverviewWindow : TabWindow
{
    public MarketOverviewWindow(Client c) : base(c)
    {
        var prices = new MarketPricesOverview(c);
        AddTab(prices);
        var qs = new MarketQuantitiesOverview(c);
        AddTab(qs);
        MinSize = new Vector2I(700, 500);
    }

    public static void Open(Client c)
    {
        var w = new MarketOverviewWindow(c);
        Game.I.Client.WindowHolder.OpenWindow(w);
    }
}
