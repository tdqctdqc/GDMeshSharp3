using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeFoodOverview : ScrollContainer
{
    private VBoxContainer _container;
    public RegimeFoodOverview()
    {
        Name = "Agriculture";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        
        var actualProd = regime.History.ItemHistory.Latest(data.Models.Items.Food).Produced;
        var actualCons = regime.History.ItemHistory.Latest(data.Models.Items.Food).Consumed;
        _container.CreateLabelAsChild($"Last Prod: {actualProd}");
        _container.CreateLabelAsChild($"Consumption: {actualCons}");
        
        
        var populatedPolys = regime.GetPolys(data)
            .Where(p => p.HasPeep(data));
        var peeps = populatedPolys
            .Select(p => p.GetPeep(data));
        var peepCount = peeps.Count();
        var peepSize = peeps.Sum(p => p.Size);
        var jobs = populatedPolys
            .SelectMany(p => p.PolyEmployment.Counts)
            .SortInto(kvp => kvp.Key, kvp => kvp.Value);

        var techniqueCounts = populatedPolys
            .SelectMany(p => p.PolyFoodProd.Nums)
            .SortInto(p => data.Models.GetModel<FoodProdTechnique>(p.Key), p => p.Value);
        
        
        foreach (var kvp in techniqueCounts)
        {
            var technique = kvp.Key;
            var num = kvp.Value;
            var hbox = new HBoxContainer();
            hbox.AddChild(technique.Icon.GetTextureRect(Vector2.One * 50f));
            hbox.CreateLabelAsChild(num.ToString());
            _container.AddChild(hbox);

            var needed = technique.BaseLabor * num;
            var have = jobs.ContainsKey(technique.JobType.Id) ? jobs[technique.JobType.Id] : 0;
            var ratio = have / needed;
            _container.CreateLabelAsChild($"Labor: {have} / {needed}");
            _container.CreateLabelAsChild($"Expected output: {ratio * num * technique.BaseProd}");
            _container.CreateLabelAsChild($"Maximum output: {num * technique.BaseProd}");
        }
        
        
    }
}
