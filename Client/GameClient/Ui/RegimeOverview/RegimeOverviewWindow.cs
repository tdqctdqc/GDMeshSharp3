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
    }
    public void Setup(Regime regime, Data data)
    {
        _general.Setup(regime, data);
        _construction.Setup(regime, data);
        _peeps.Setup(regime, data);
        _items.Setup(regime, data);
        _ag.Setup(regime, data);
        _budget.Setup(regime, data);
        _flows.Setup(regime, data);
        _manuf.Setup(regime, data);
    }
}