using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class FoodTab : ScrollContainer, IUiDrawable
{
    private VBoxContainer _container;
    private RegimeOverviewWindow _parent;
    public FoodTab(RegimeOverviewWindow parent)
    {
        _parent = parent;
        Name = "Food";
        _container = new VBoxContainer();
        _container.FullRect();
        AddChild(_container);
    }
    public void Draw(Client client)
    {
        _container.ClearChildren();
        var regime = _parent.Regime;
        if (regime is null) return;
        var populatedCells = regime.GetCells(client.Data)
            .OfType<LandCell>().Where(c => c.HasPeep(client.Data));
        var peeps = populatedCells
            .Select(p => p.GetPeep(client.Data));
        var peepCount = peeps.Count();
        var peepSize = peeps.Sum(p => p.Size);
        var jobs = populatedCells
            .Select(p => p.GetPeep(client.Data))
            .SelectMany(p => p.Employment.Counts.GetEnumModel(client.Data))
            .SortInto(kvp => kvp.Key, kvp => kvp.Value);

        var techniqueCounts = 
            populatedCells
            .SelectMany(p => p.FoodProd.Nums.GetEnumModel(client.Data))
            .SortInto(p => p.Key, p => p.Value);
        
        var iconSize = client.Settings.MedIconSize.Value;

        foreach (var kvp in techniqueCounts)
        {
            var technique = kvp.Key;
            var num = kvp.Value;

            var box = technique.Icon.GetLabeledIcon<HBoxContainer>(num.ToString(), iconSize);
            _container.AddChild(box);

            var needed = technique.BaseLabor * num;
            var have = jobs.ContainsKey(technique.JobType) ? jobs[technique.JobType] : 0;
            var ratio = have / needed;
            _container.CreateLabelAsChild($"Labor: {have} / {needed}");
            _container.CreateLabelAsChild($"Expected output: {ratio * num * technique.BaseProd}");
            _container.CreateLabelAsChild($"Maximum output: {num * technique.BaseProd}");
        }
        
        
    }
}
