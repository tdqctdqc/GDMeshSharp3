using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

public partial class RegimePeepsInfoBar : HBoxContainer
{
    public RegimePeepsInfoBar(Client client, Data data)
    {
        var sizeLabel = new Label();
        var deltaLabel = new Label();
        var popSize = StatLabel.Construct<int>(client, "Pop Size", sizeLabel,
            () => GetPopulationCount(data));
        var notices = client.Notices;
        popSize.AddTrigger(notices.ChangedSpectatingRegime.Blank);
        popSize.AddTrigger(data.Notices.Ticked.Blank);
        popSize.AddTrigger(data.Notices.FinishedTurnStartCalc);
        AddChild(sizeLabel);
        var popGrowth = StatLabel.Construct<int>(client, 
            "Pop Growth", deltaLabel,
            () => GetPeepDelta(data));
        popGrowth.AddTrigger(notices.ChangedSpectatingRegime.Blank);
        popGrowth.AddTrigger(data.Notices.Ticked.Blank);
        popGrowth.AddTrigger(data.Notices.FinishedTurnStartCalc);

        AddChild(deltaLabel);
    }

    private int GetPopulationCount(Data data)
    {
        var r = Game.I.Client.GetComponent<MapGraphics>().SpectatingRegime;
        return r.GetPopulation(data);
    }

    private int GetPeepDelta(Data data)
    {
        var r = Game.I.Client.GetComponent<MapGraphics>().SpectatingRegime;
        var ordered = r.History.PeepHistory.GetOrdered();
        if (ordered.Count > 1)
        {
            var last = ordered[ordered.Count - 1].TotalPop;
            var penult = ordered[ordered.Count - 2].TotalPop;
            return Mathf.FloorToInt(last - penult);
        }

        return 0;
    }
}
