using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

public partial class RegimePeepsInfoBar : HBoxContainer
{
    public RegimePeepsInfoBar(Data data)
    {
        var sizeLabel = new Label();
        var deltaLabel = new Label();
        var popSize = StatLabel.Construct<int>("Pop Size", sizeLabel,
            () => GetPopulationCount(data));
        popSize.AddTrigger(data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        popSize.AddTrigger(data.Notices.Ticked.Blank);
        popSize.AddTrigger(data.Notices.FinishedTurnStartCalc);
        AddChild(sizeLabel);
        var popGrowth = StatLabel.Construct<int>("Pop Growth", deltaLabel,
            () => GetPeepDelta(data));
        popGrowth.AddTrigger(data.BaseDomain.PlayerAux.PlayerChangedRegime.Blank);
        popGrowth.AddTrigger(data.Notices.Ticked.Blank);
        popGrowth.AddTrigger(data.Notices.FinishedTurnStartCalc);

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
