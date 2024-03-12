using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.RegimeOverview;

public partial class RegimeOverviewWindow : TabWindow
{
    private GeneralTab _general;
    private PeepsTab _peeps;
    private ItemsTab _items;
    private FoodTab _ag;
    private BudgetTab _budget;
    private FlowsTab _flows;
    private MakingTab _manuf;
    private MilitaryTab _troop;
    public RegimeOverviewWindow()
    {
        MinSize = new Vector2I(1000, 1000);

        _general = new GeneralTab();
        AddTab(_general);

        _peeps = new PeepsTab();
        AddTab(_peeps);

        _items = new ItemsTab();
        AddTab(_items);

        _ag = new FoodTab();
        AddTab(_ag);

        _budget = new BudgetTab();
        AddTab(_budget);

        _flows = new FlowsTab();
        AddTab(_flows);

        _manuf = new MakingTab();
        AddTab(_manuf);

        _troop = new MilitaryTab();
        AddTab(_troop);
    }
    public void Setup(Regime regime, Client client)
    {
        _general.Setup(regime, client);
        _peeps.Setup(regime, client);
        _items.Setup(regime, client);
        _ag.Setup(regime, client);
        _budget.Setup(regime, client);
        _flows.Setup(regime, client);
        _manuf.Setup(regime, client);
        _troop.Setup(regime, client);
    }
}