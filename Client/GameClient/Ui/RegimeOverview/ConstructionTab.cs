using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.RegimeOverview;

public partial class ConstructionTab : ScrollContainer
{
    private VBoxContainer _container;
    public ConstructionTab()
    {
        Name = "Construction";
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _container.ClearChildren();
        var constructions = data.Infrastructure.CurrentConstruction
            .ByPoly.Where(kvp => regime.GetPolys(data).Contains(data.Get<MapPolygon>(kvp.Key)))
            .SelectMany(kvp => kvp.Value).ToList();
        var constrFlow = regime.Flows.Get(data.Models.Flows.ConstructionCap);
        _container.CreateLabelAsChild($"Cap: {constrFlow.FlowIn}");
        _container.CreateLabelAsChild($"In Use: {constrFlow.FlowOut}");
        _container.CreateLabelAsChild($"Available: {constrFlow.Net()}");
        
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
