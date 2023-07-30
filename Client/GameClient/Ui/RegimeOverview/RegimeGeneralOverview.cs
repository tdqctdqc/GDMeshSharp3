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
        AddChild(_container);
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
            var button = new Button();
            button.Text = "Choose Regime";
            button.Pressed += () =>
            {
                var com = new ChooseRegimeCommand(regime.MakeRef(),
                    data.ClientPlayerData.LocalPlayerGuid);
                Game.I.Client.HandleCommand(com);
            };
            _container.AddChild(button);
        }
        
        _container.CreateLabelAsChild("RIVALS");
        foreach (var rival in regime.GetAlliance(data).Rivals.Items(data))
        {
            _container.CreateLabelAsChild(rival.Leader.Entity(data).Name);
        }
    }
}
