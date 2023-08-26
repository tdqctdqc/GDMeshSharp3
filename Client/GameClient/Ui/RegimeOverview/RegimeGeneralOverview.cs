using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeGeneralOverview : ScrollContainer
{
    private VBoxContainer _container;
    public override void _Ready()
    {
        base._Ready();
    }

    public RegimeGeneralOverview()
    {
        AnchorsPreset = (int)LayoutPreset.FullRect;
        _container = new VBoxContainer();
        AddChild(_container);
        _container.AnchorsPreset = (int)LayoutPreset.FullRect;
    }
    public void Setup(Regime regime, Data data)
    {
        Name = regime.Name;
        _container.ClearChildren();
        var flagControl = new Control();
        var regimeFlagRect = new TextureRect();
        regimeFlagRect.ExpandMode = TextureRect.ExpandModeEnum.FitHeightProportional;
        regimeFlagRect.StretchMode = TextureRect.StretchModeEnum.Scale;
        regimeFlagRect.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        regimeFlagRect.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        regimeFlagRect.Texture = regime.Template.Model(data).Flag;
        regimeFlagRect.CustomMinimumSize = new Vector2(150f, 100f);
        _container.AddChild(regimeFlagRect);
        
        if (regime.IsPlayerRegime(data) == false)
        {
            var button = ButtonExt.GetButton(() =>
            {
                var com = new ChooseRegimeCommand(regime.MakeRef(),
                    data.ClientPlayerData.LocalPlayerGuid);
                Game.I.Client.HandleCommand(com);
            });
            button.Text = "Choose Regime";
            _container.AddChild(button);
        }

        _container.CreateLabelAsChild("ALLIANCE: " + regime.GetAlliance(data).Id);
        _container.CreateLabelAsChild("ALLIANCE LEADER: " 
                                      + regime.GetAlliance(data).Leader.Entity(data).Name
                                      + " " + regime.GetAlliance(data).Leader.Entity(data).Id);
        _container.CreateLabelAsChild("RIVALS");
        foreach (var rival in regime.GetAlliance(data).Rivals.Items(data))
        {
            _container.CreateLabelAsChild(rival.Leader.Entity(data).Name);
        }
    }
}
