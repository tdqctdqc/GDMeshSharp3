using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class PeepsTab : ScrollContainer
{
    private VBoxContainer _container;
    public PeepsTab()
    {
        Name = "Peeps";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
        AddChild(_container);
    }
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        var populatedCells = regime.GetCells(client.Data)
            .Where(p => p.HasPeep(client.Data));
        var peeps = populatedCells
            .Select(p => p.GetPeep(client.Data));
        var peepCount = peeps.Count();
        var peepSize = peeps.Sum(p => p.Size);
        var jobs = populatedCells
            .Select(p => p.GetPeep(client.Data))
            .SelectMany(p => p.Employment.Counts)
            .SortInto(kvp => kvp.Key, kvp => kvp.Value);
        _container.CreateLabelAsChild("Peeps: " + peepCount);
        _container.CreateLabelAsChild("Population: " + peepSize);
        var iconSize = client.Settings.MedIconSize.Value;

        foreach (var kvp in jobs.OrderByDescending(k => k.Value))
        {
            var job = (PeepJob)client.Data.Models[kvp.Key];
            var count = kvp.Value;
            var hbox = job.Icon.GetLabeledIcon<HBoxContainer>(
                count.ToString(), iconSize);
            _container.AddChild(hbox);
        }
    }
}
