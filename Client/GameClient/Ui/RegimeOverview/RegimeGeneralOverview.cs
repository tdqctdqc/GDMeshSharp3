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
