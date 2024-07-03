using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class CombatResultsGraphic : ScrollContainer
{
    public UnitCombatInfo Selected => _token.Value;
    private SelectableControlListToken<UnitCombatInfo> _token;
    public void Draw(IEnumerable<UnitCombatInfo> infos, Data d)
    {
        this.ClearChildren();
        this.ExpandFill();

        var inner = new VBoxContainer();
        inner.AnchorsPreset = (int)LayoutPreset.HcenterWide;
        AddChild(inner);
        
        _token = new SelectableControlListToken<UnitCombatInfo>(
            i => GetEntry(
                i.Template.Fulfilled() 
                    ? $"{i.Template.Get(d).Name} {i.Unit.RefId}" 
                    : "Anonymous",
                i.Initial,
                i.Active, i.Kills, d),
            i => { }
        );
        
        _token.Node.ExpandFill();
        inner.AddChild(_token.Node);
        foreach (var unitCombatInfo in infos)
        {
            _token.Add(unitCombatInfo);
        }
    }

    private Control GetEntry(string unitName, IdCount<Troop> initials, 
        IdCount<Troop> actives, IdCount<Troop> kills, Data d)
    {
        var res = new VBoxContainer();
        var l = res.CreateLabelAsChild(unitName);
        l.CustomMinimumSize = new Vector2(100f, 10f);
        res.SetAnchorsPreset(LayoutPreset.HcenterWide);
        foreach (var (troop, amt) in initials.GetEnumModel(d))
        {
            var label = new Label();
            var active = actives.Get(troop);
            label.Text = $"{troop.DisplayName}: {active.RoundTo2Digits()}/{amt.RoundTo2Digits()}";
            label.SetAnchorsPreset(LayoutPreset.HcenterWide);
            label.ExpandFill();
            res.AddChild(label);
            var pics = GetSubEntry(troop, active, amt, "Active");
            pics.ExpandFill();
            res.AddChild(pics);
        }
        res.ExpandFill();
        return res;
    }

    private Control GetSubEntry(Troop t, float active, float initial, string text)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var maxRows = 5;

        var pics = new FlexIconDisplay(this,
            Vector2.One * size,
            new Vector2(_token.Node.Size.X, maxRows * size));

        var activeCap = Mathf.CeilToInt(active);
        var initialCap = Mathf.CeilToInt(initial);
        var icons = Enumerable.Range(0, initialCap)
            .Select(i => t.Icon.GetTextureRect(size)).ToList<Control>();
        for (var i = activeCap; i < initialCap; i++)
        {
            icons[i].Modulate = Colors.Red;
        }
        pics.SetChildren(icons);
        return pics;
    }
    
    
    
}