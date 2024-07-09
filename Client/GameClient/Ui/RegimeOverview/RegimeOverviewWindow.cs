using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.RegimeOverview;

public partial class RegimeOverviewWindow 
    : TabWindow
{
    public Regime Regime { get; private set; }
    public RegimeOverviewWindow(Client c) : base(c)
    {
        AddTab(new GeneralTab(this));
        AddTab(new PeepsTab(this));
        AddTab(new BudgetTab(this));
        AddTab(new EconomyTab(this, c));
        AddTab(new MilitaryTab(() => Regime, c));
    }
    public void Setup(Regime regime)
    {
        Regime = regime;
    }

    public static RegimeOverviewWindow Open(Regime r, Client client)
    {
        var w = new RegimeOverviewWindow(client);
        w.Setup(r);
        client.WindowHolder.OpenWindow(w);
        return w;
    }
}