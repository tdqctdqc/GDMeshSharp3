using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeConstructionOverview : ScrollContainer
{
    private VBoxContainer _container;
    public RegimeConstructionOverview()
    {
        Name = "Construction";
        SetAnchorsPreset(LayoutPreset.FullRect);
        _container = new VBoxContainer();
        _container.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var constructions = data.Infrastructure.CurrentConstruction
            .ByPoly.Where(kvp => regime.GetPolys(data).Contains(data.Get<MapPolygon>(kvp.Key)))
            .SelectMany(kvp => kvp.Value).ToList();
        _container.CreateLabelAsChild($"Cap: {regime.Flows[data.Models.Flows.ConstructionCap].FlowIn}");
        _container.CreateLabelAsChild($"In Use: {regime.Flows[data.Models.Flows.ConstructionCap].FlowOut}");
        _container.CreateLabelAsChild($"Available: {regime.Flows[data.Models.Flows.ConstructionCap].Net()}");
        
        foreach (var construction in constructions)
        {
            var hbox = new HBoxContainer();
            var building = construction.Model.Model(data);
            hbox.AddChild(building.Icon.GetTextureRect(Vector2.One * 50f));
            var ticksDone = construction.TicksDone(data);
            hbox.CreateLabelAsChild($"{ticksDone} / {building.NumTicksToBuild}");
            _container.AddChild(hbox);
        }
    }
}
