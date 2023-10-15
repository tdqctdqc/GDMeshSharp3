using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
namespace Ui.RegimeOverview;

public partial class FoodTab : ScrollContainer
{
    private VBoxContainer _container;
    public FoodTab()
    {
        Name = "Food";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
        AddChild(_container);
    }
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        
        var actualProd = regime.History.ItemHistory.GetLatest(client.Data.Models.Items.Food).Produced;
        var actualCons = regime.History.ItemHistory.GetLatest(client.Data.Models.Items.Food).Consumed;
        var demand = regime.GetPeeps(client.Data).Sum(p => p.Size)
                     * client.Data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        _container.CreateLabelAsChild($"Last Prod: {actualProd}");
        _container.CreateLabelAsChild($"Consumption: {actualCons}");
        _container.CreateLabelAsChild($"Net: {actualProd - actualCons}");
        _container.CreateLabelAsChild($"Demand: {demand}");
        if (demand > actualCons)
        {
            _container.CreateLabelAsChild($"Deficit of {demand - actualCons}");
        }
        
        
        
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

        var techniqueCounts = populatedPolys
            .SelectMany(p => p.PolyFoodProd.Nums)
            .SortInto(p => client.Data.Models.GetModel<FoodProdTechnique>(p.Key), p => p.Value);
        
        
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
