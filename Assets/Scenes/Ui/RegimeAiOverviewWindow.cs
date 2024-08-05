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
            var control = GetRegimeAiOverview(regime, ai, _data);
            control.Name = regime.Name;
            _container.AddChild(control);
        }
    }

    private Control GetRegimeAiOverview(Regime r, RegimeAi ai, Data data)
    {
        var vbox = new VBoxContainer();
        vbox.AnchorsPreset = (int)Control.LayoutPreset.FullRect;
        vbox.CreateLabelAsChild(r.Name);
        for (var i = 0; i < ai.Status.Count; i++)
        {
            var label = new Label();
            label.Text += "....";

            label.Text += ai.Status[i];
            vbox.AddChild(label);
        }

        if (ai.Status.Count > 0 && ai.Status.Last() == "Finished")
        {
            vbox.Modulate = Colors.Green;
        }
        else
        {
            vbox.Modulate = Colors.Red;
        }
        
        return vbox;
    }
}
