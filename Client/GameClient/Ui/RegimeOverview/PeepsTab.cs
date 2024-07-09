using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class PeepsTab : ScrollContainer, IUiDrawable
{
    private Container _container;
    private RegimeOverviewWindow _parent;
    public PeepsTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Peeps";
        _container = this.MakeScrollChild<HBoxContainer>(
            out var scroll);
        _container.ExpandFill();
        scroll.ExpandFill();
    }

    private PeepsTab()
    {
    }

    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;

        var left = _container.MakeScrollChild<VBoxContainer>(
            out var leftScroll);
        left.ExpandFill();
        leftScroll.ExpandFill();

        var right = _container.MakeScrollChild<VBoxContainer>(
            out var rightScroll);
        right.ExpandFill();
        rightScroll.ExpandFill();
        var populatedCells = regime.GetCells(client.Data)
            .Where(p => p.HasPeep(client.Data));
        var settlements = populatedCells.Where(c => c.HasSettlement(client.Data))
            .Select(c => c.GetSettlement(client.Data));
        var peeps = regime.GetPeeps(client.Data);
        var peepCount = peeps.Count();
        var pop = peeps.Sum(p => p.Size);
        left.CreateLabelAsChild("Peeps: " + peepCount);
        left.CreateLabelAsChild("Population: " + pop);
        var urban = settlements.Sum(s => s.Cell.Get(client.Data).GetPeep(client.Data).Size);
        var rural = pop - urban;
        left.CreateLabelAsChild($"Urban: {urban}");
        left.CreateLabelAsChild($"Rural: {rural}");
        
        
        var iconSize = client.Settings.MedIconSize.Value;
        var jobs = populatedCells
            .Select(p => p.GetPeep(client.Data))
            .SelectMany(p => p.Employment.Counts.GetEnumModel(client.Data))
            .SortInto(kvp => kvp.Key, kvp => kvp.Value);

        foreach (var (job, count) in jobs.OrderByDescending(k => k.Value))
        {
            var hbox = job.Icon.GetLabeledIcon<HBoxContainer>(
                count.ToString(), iconSize);
            left.AddChild(hbox);
        }
    }
}
