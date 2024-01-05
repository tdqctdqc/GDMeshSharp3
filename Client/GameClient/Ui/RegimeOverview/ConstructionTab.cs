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
    public void Setup(Regime regime, Client client)
    {
        _container.ClearChildren();
        var iconSize = client.Settings.MedIconSize.Value;
        var constructions = client.Data.Infrastructure.CurrentConstruction
            .ByPoly.Where(kvp => regime.GetPolys(client.Data).Contains(client.Data.Get<MapPolygon>(kvp.Key)))
            .SelectMany(kvp => kvp.Value).ToList();
        var constrFlow = regime.Flows.Get(client.Data.Models.Flows.ConstructionCap);
        _container.CreateLabelAsChild($"Cap: {constrFlow.FlowIn}");
        _container.CreateLabelAsChild($"In Use: {constrFlow.FlowOut}");
        _container.CreateLabelAsChild($"Available: {constrFlow.Net()}");
        
        foreach (var construction in constructions)
        {
            var building = construction.Model.Model(client.Data);
            var ticksDone = construction.TicksDone(client.Data);
            var box = NodeExt.GetLabeledIcon<HBoxContainer>(
                building.Icon, $"{ticksDone} / {building.NumTicksToBuild}",
                iconSize);
            
            _container.AddChild(box);
        }
    }
}
