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
        Game.I.Client.WindowHolder.OpenWindow(w);
    }

    private RegimeAiOverviewWindow()
    {
        _container = this.MakeWholeWindowScrollContainer<VBoxContainer>(Vector2I.One * 500);
        this.MakeFreeable();
        AboutToPopup += Draw;
        Size = Vector2I.One * 500;
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
            for (var j = 0; j < i + 1; j++)
            {
                label.Text += "\t";
                
            }

            label.Text += ai.Status[i];
            vbox.AddChild(label);
        }
        
        return vbox;
    }
}
