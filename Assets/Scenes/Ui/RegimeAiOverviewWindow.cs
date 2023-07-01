using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RegimeAiOverviewWindow : TabWindow
{
    private EntityValueCache<Regime, RegimeAi> _aiRegimes;
    private Data _data;
    public static RegimeAiOverviewWindow Get(Data data)
    {
        var res = new RegimeAiOverviewWindow();
        res._data = data;
        res._aiRegimes = data.HostLogicData.AIs;
        return res;
    }

    private RegimeAiOverviewWindow()
    {
        AboutToPopup += Draw;
        Size = Vector2I.One * 500;
    }

    public void Draw()
    {
        Clear();
        foreach (var kvp in _aiRegimes.Dic)
        {
            var regime = kvp.Key;
            var ai = kvp.Value;
            var control = GetRegimeAiOverview(regime, ai, _data);
            control.Name = regime.Name;
            AddTab(control);
        }
    }

    private Control GetRegimeAiOverview(Regime r, RegimeAi ai, Data data)
    {
        var c = new ScrollContainer();
        var vbox = new VBoxContainer();
        c.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        c.AddChild(vbox);
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.CreateLabelAsChild("WISHLIST");
        var wishlist = ai.Budget.GetItemWishlist(_data);
        foreach (var kvp in wishlist)
        {
            var item = kvp.Key;
            var num = kvp.Value;
            vbox.CreateLabelAsChild($"{item.Name}: {num}");
        }
        return c;
    }
}
