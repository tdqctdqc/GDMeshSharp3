using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.RegimeOverview;

public partial class RegimeOverviewWindow 
    : TabWindow
{
    public Regime Regime { get; private set; }
    private GeneralTab _general;
    private PeepsTab _peeps;
    private StockTab _stock;
    private FoodTab _ag;
    private BudgetTab _budget;
    private FlowsTab _flows;
    private MakingTab _manuf;
    private MilitaryTab _troop;
    public RegimeOverviewWindow(Client c) : base(c)
    {
        _general = new GeneralTab(this);
        AddTab(_general);

        _peeps = new PeepsTab(this);
        AddTab(_peeps);

        _stock = new StockTab(this);
        AddTab(_stock);

        _ag = new FoodTab(this);
        AddTab(_ag);

        _budget = new BudgetTab(this);
        AddTab(_budget);

        _flows = new FlowsTab(this);
        AddTab(_flows);

        _manuf = new MakingTab(this);
        AddTab(_manuf);

        _troop = new MilitaryTab(() => Regime, c);
        AddTab(_troop);
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