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
        var populatedPolys = regime.GetPolys(client.Data)
            .Where(p => p.HasPeep(client.Data));
        var peeps = populatedPolys
            .Select(p => p.GetPeep(client.Data));
        var peepCount = peeps.Count();
        var peepSize = peeps.Sum(p => p.Size);
        var jobs = populatedPolys
            .Select(p => p.GetPeep(client.Data))
            .SelectMany(p => p.Employment.Counts)
            .SortInto(kvp => kvp.Key, kvp => kvp.Value);
        _container.CreateLabelAsChild("Peeps: " + peepCount);
        _container.CreateLabelAsChild("Population: " + peepSize);
        
        foreach (var kvp in jobs.OrderByDescending(k => k.Value))
        {
            var hbox = new HBoxContainer();
            var job = (PeepJob)client.Data.Models[kvp.Key];
            var count = kvp.Value;
            hbox.AddChild(job.JobIcon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild(count.ToString());
            _container.AddChild(hbox);
        }
    }
}
