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
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var populatedPolys = regime.GetPolys(data)
            .Where(p => p.HasPeep(data));
        var peeps = populatedPolys
            .Select(p => p.GetPeep(data));
        var peepCount = peeps.Count();
        var peepSize = peeps.Sum(p => p.Size);
        var jobs = populatedPolys
            .Select(p => p.GetPeep(data))
            .SelectMany(p => p.Employment.Counts)
            .SortInto(kvp => kvp.Key, kvp => kvp.Value);
        _container.CreateLabelAsChild("Peeps: " + peepCount);
        _container.CreateLabelAsChild("Population: " + peepSize);
        
        foreach (var kvp in jobs.OrderByDescending(k => k.Value))
        {
            var hbox = new HBoxContainer();
            var job = (PeepJob)data.Models[kvp.Key];
            var count = kvp.Value;
            hbox.AddChild(job.JobIcon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild(count.ToString());
            _container.AddChild(hbox);
        }
    }
}
