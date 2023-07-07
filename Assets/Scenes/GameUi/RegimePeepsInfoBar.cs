using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

public partial class RegimePeepsInfoBar : HBoxContainer
{
    private RefAction _update;
    public void Setup(Data data)
    {
        var sizeLabel = new Label();
        var deltaLabel = new Label();
        var popSize = StatLabel.Construct<int>("Pop Size", sizeLabel,
            () => GetPopulationCount(data));
        popSize.AddTrigger(data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        popSize.AddTrigger(data.Notices.Ticked.Blank);
        AddChild(sizeLabel);
        var popGrowth = StatLabel.Construct<int>("Pop Growth", deltaLabel,
            () => GetPeepDelta(data));
        popGrowth.AddTrigger(data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        popGrowth.AddTrigger(data.Notices.Ticked.Blank);

        AddChild(deltaLabel);
    }

    private int GetPopulationCount(Data data)
    {
        var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
        if (r.Empty() == false)
        {
            return r.Entity(data).GetPopulation(data);
        }

        return 0;
    }

    private int GetPeepDelta(Data data)
    {
        var r = data.BaseDomain.PlayerAux.LocalPlayer.Regime;
        if (r.Empty() == false)
        {
            return Mathf.FloorToInt(r.Entity(data).History.PeepHistory.PeepSize.GetLatestDelta());
        }

        return 0;
    }
}
