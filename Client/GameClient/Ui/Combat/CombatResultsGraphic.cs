using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class CombatResultsGraphic : ScrollContainer
{
    private VBoxContainer _inner;

    public void Draw(IEnumerable<UnitCombatInfo> infos, Data d)
    {
        this.ClearChildren();
        this.ExpandFill();
        
        _inner = new VBoxContainer();
        _inner.AnchorsPreset = (int)LayoutPreset.HcenterWide;
        AddChild(_inner);
        
        foreach (var unitCombatInfo in infos)
        {
            var entry = GetEntry(unitCombatInfo.Initial,
                unitCombatInfo.Active, unitCombatInfo.Kills, d);
            _inner.AddChild(entry);
        }
        
        if (infos.Count() > 1)
        {
            this.CreateLabelAsChild("TOTALS");
            var initials = IdCount<Troop>.Sum(infos.Select(i => i.Initial).ToArray());
            var actives = IdCount<Troop>.Sum(infos.Select(i => i.Initial).ToArray());
            var kills = IdCount<Troop>.Sum(infos.Select(i => i.Initial).ToArray());
            var entry = GetEntry(initials, actives, kills, d);
            _inner.AddChild(entry);
        }
    }

    private VBoxContainer GetEntry(IdCount<Troop> initials, 
        IdCount<Troop> actives, IdCount<Troop> kills, Data d)
    {
        var res = new VBoxContainer();
        res.SetAnchorsPreset(LayoutPreset.HcenterWide);
        foreach (var (troop, amt) in initials.GetEnumModel(d))
        {
            var line = new VBoxContainer();
            line.SetAnchorsPreset(LayoutPreset.HcenterWide);
            var label = new Label();
            label.Text = troop.Name;
            label.SetAnchorsPreset(LayoutPreset.HcenterWide);
            line.AddChild(label);
            res.AddChild(line);
            var initialLine = GetSubEntry(troop, amt, "Initial");
            var activeLine = GetSubEntry(troop, actives.Get(troop), "Active");
            line.AddChild(initialLine);
            line.AddChild(activeLine);
        }

        return res;
    }

    private VBoxContainer GetSubEntry(Troop t, float amt, string text)
    {
        var box = new VBoxContainer();
        box.SetAnchorsPreset(LayoutPreset.HcenterWide);
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var label = new Label();
        label.Text = $"{text}: {amt}";
        box.AddChild(label);
        box.SetAnchorsPreset(LayoutPreset.HcenterWide);
        var maxRows = 5;

        var pics = new FlexIconDisplay(this,
            Vector2.One * size,
            new Vector2(_inner.Size.X, maxRows * size));
        box.AddChild(pics);
        var icons = Enumerable.Range(0, Mathf.CeilToInt(amt))
            .Select(i => t.Icon.GetTextureRect(size));
        pics.SetChildren(icons.ToList<Control>());
        return box;
    }
    
    
    
}