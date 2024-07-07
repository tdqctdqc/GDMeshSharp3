using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Ui.Combat;
using Ui.MilitaryWindow;

namespace Ui.RegimeOverview;

public partial class MilitaryTab : DrawTabContainer, IUiDrawable
{
    public MilitaryTab(Func<Regime> getRegime, Client c)
        : base(c)
    {
        Name = "Military";
        this.FullRect();
        AddTab(new ArmiesTab(getRegime));
        AddTab(new MakeUnitsTab(getRegime));
        AddTab(new MakeTroopsTab(getRegime));
    }
}