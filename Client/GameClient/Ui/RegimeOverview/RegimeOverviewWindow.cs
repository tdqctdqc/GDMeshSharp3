using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.RegimeOverview;

public partial class RegimeOverviewWindow : TabWindow
{
    private GeneralTab _general;
    private ConstructionTab _construction;
    private PeepsTab _peeps;
    private ItemsTab _items;
    private FoodTab _ag;
    private BudgetTab _budget;
    private FlowsTab _flows;
    private ManufacturingTab _manuf;
    private TroopReserveTab _troop;
    public RegimeOverviewWindow()
    {
        MinSize = new Vector2I(1000, 1000);

        _general = new GeneralTab();
        AddTab(_general);
        
        _construction = new ConstructionTab();
        AddTab(_construction);

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

        _manuf = new ManufacturingTab();
        AddTab(_manuf);

        _troop = new TroopReserveTab();
        AddTab(_troop);
    }
    public void Setup(Regime regime, Client client)
    {
        _general.Setup(regime, client);
        _construction.Setup(regime, client);
        _peeps.Setup(regime, client);
        _items.Setup(regime, client);
        _ag.Setup(regime, client);
        _budget.Setup(regime, client);
        _flows.Setup(regime, client);
        _manuf.Setup(regime, client);
        _troop.Setup(regime, client);
    }
}