using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeAiOverviewWindow : Window
{
    private Cache<Regime, RegimeAi> _aiRegimes;
    private VBoxContainer _container;
    private Data _data;
    public static void Open(Data data)
    {
        var w = new RegimeAiOverviewWindow();
        w._data = data;
        w._aiRegimes = data.HostLogicData.RegimeAis;
        Game.I.Client.WindowHolder.OpenWindowFullSize(w);
    }

    private RegimeAiOverviewWindow()
    {
        var panelContainer = new PanelContainer();
        AddChild(panelContainer);
        panelContainer.FullRect();
        panelContainer.ExpandFill();
        _container = panelContainer.MakeScrollChild<VBoxContainer>(out var scroll);
        _container.FullRect();
        _container.ExpandFill();
        this.MakeFreeable();
        AboutToPopup += Draw;
    }

    public void Draw()
    {
        _container.ClearChildren();
        foreach (var kvp in _aiRegimes.Dic)
        {
            var regime = kvp.Key;
            var ai = kvp.Value;
            var control = ai.Timer.GetNode();
            control.Modulate = ai.Calculating ? Colors.Red : Colors.Green;
            _container.AddChild(control);
        }
    }
}
