using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeOverviewWindow : TabWindow
{
    private RegimeGeneralOverview _general;
    private RegimeConstructionOverview _construction;
    private RegimePeepsOverview _peeps;
    private RegimeWalletOverview _wallet;
    private RegimeFoodOverview _ag;
    public RegimeOverviewWindow()
    {
        MinSize = new Vector2I(500, 500);

        _general = new RegimeGeneralOverview();
        AddTab(_general);
        
        _construction = new RegimeConstructionOverview();
        AddTab(_construction);

        _peeps = new RegimePeepsOverview();
        AddTab(_peeps);

        _wallet = new RegimeWalletOverview();
        AddTab(_wallet);

        _ag = new RegimeFoodOverview();
        AddTab(_ag);
    }
    public void Setup(Regime regime, Data data)
    {
        _general.Setup(regime, data);
        _construction.Setup(regime, data);
        _peeps.Setup(regime, data);
        _wallet.Setup(regime, data);
        _ag.Setup(regime, data);
    }
}