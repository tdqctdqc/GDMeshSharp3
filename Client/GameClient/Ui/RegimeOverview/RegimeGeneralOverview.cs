using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeGeneralOverview : ScrollContainer
{
    private VBoxContainer _container;
    public RegimeGeneralOverview()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        _container = new VBoxContainer();
        _container.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_container);
    }
    public void Setup(Regime regime, ClientWriteKey key)
    {
        Name = regime.Name;
        _container.ClearChildren();
        var regimeFlagRect = new TextureRect();
        regimeFlagRect.Size = new Vector2(30f, 20f);
        regimeFlagRect.CustomMinimumSize = new Vector2(30f, 20f);
        regimeFlagRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
        regimeFlagRect.Texture = regime.Template.Model(key.Data).Flag;
        _container.AddChild(regimeFlagRect);
        _container.CreateLabelAsChild("ENEMIES");
        foreach (var enemy in regime.GetAlliance(key.Data).Enemies.Entities(key.Data))
        {
            _container.CreateLabelAsChild(enemy.Leader.Entity(key.Data).Name);
        }
        if (regime.IsPlayerRegime(key.Data) == false)
        {
            var button = new Button();
            button.Text = "Choose Regime";
            button.Pressed += () =>
            {
                var com = new ChooseRegimeCommand(regime.MakeRef());
                key.Session.Server.QueueCommandLocal(com);
            };
            _container.AddChild(button);
        }
    }
}
