using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class UnitsTab : HBoxContainer, IUiDrawable
{
    private CombatInfo _info;
    private ItemListToken<UnitCombatInfo> _defendersList, _attackersList;
    public UnitsTab(CombatInfo info)
    {
        _info = info;
        Name = "Units";
    }

    public void Draw(Client c)
    {
        this.ClearChildren();

        var scrolls = new VBoxContainer();
        scrolls.ExpandFill();

        var info = new VBoxContainer();
        info.ExpandFill();
        
        
        var size = c.Settings.MedIconSize.Value * Vector2.One;

        var atkInfos = _info.Attackers;
        
        var attackers = new VBoxContainer();
        attackers.ExpandFill();
        attackers.CreateLabelAsChild("Attackers");
        _attackersList = new ItemListToken<UnitCombatInfo>(
            atkInfos,
            u =>
            {
                return u.Template.Fulfilled()
                    ? u.Template.Get(c.Data).Name + " " + u.Unit
                    : "None";
            },
            false
        );
        attackers.AddChild(_attackersList.ItemList);
        _attackersList.ItemList.ExpandFill();
        _attackersList.JustSelected += () =>
        {
            DrawInfo(true, info, c);
        };
        
        var defenders = new VBoxContainer();
        defenders.ExpandFill();
        defenders.CreateLabelAsChild("Defenders");
        _defendersList = new ItemListToken<UnitCombatInfo>(
            _info.Defenders,
            u =>
            {
                return u.Template.Fulfilled()
                    ? u.Template.Get(c.Data).Name + " " + u.Unit
                    : "None";
            },
            false
        );
        _defendersList.JustSelected += () =>
        {
            DrawInfo(false, info, c);
        };
        _defendersList.ItemList.ExpandFill();
        defenders.AddChild(_defendersList.ItemList);
        scrolls.AddChild(attackers);
        scrolls.AddChild(defenders);
        AddChild(scrolls);
        AddChild(info);
    }

    private void DrawInfo(bool attacker,
        Control info,
        Client c)
    {
        info.ClearChildren();

        var list = attacker ? _attackersList : _defendersList;
        if (list.Values.Count != 1) return;
        var u = list.Values.First();
        
        var large = c.Settings.LargeIconSize.Value;
        var med = c.Settings.MedIconSize.Value;
        
        var template = u.Template.Get(c.Data);
        var icon = u.Initial.GetMaxPowerTroop(c.Data).Icon
            .GetLabeledIcon<HBoxContainer>(
                $"{(template is not null ? template.Name : "None")} {u.Unit}",
                large);
        info.AddChild(icon);
        info.CreateLabelAsChild($"{(attacker ? "Attacker" : "Defender")}");
        
        var losses = u.GetLosses();
        
        info.CreateLabelAsChild("Troops engaged and lost");
        
        var troopsScrollInner = info.MakeScrollChild<VBoxContainer>(
            out var troopsScroll);
        troopsScroll.CustomMinimumSize = Vector2.One * 200f;
        troopsScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        troopsScrollInner.ExpandFill();
        foreach (var (troop, amt) in u.Initial.GetEnumModel(c.Data))
        {
            var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"Deployed: {amt} Losses: {losses.Get(troop)}",
                med);
            troopsScrollInner.AddChild(entry);
        }
        
        info.CreateLabelAsChild("Troops killed");
        var killsScrollInner = info.MakeScrollChild<VBoxContainer>(
            out var killsScroll);
        killsScroll.CustomMinimumSize = Vector2.One * 200f;
        killsScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        
        
        
        foreach (var (troop, amt) in u.Kills.GetEnumModel(c.Data))
        {
            var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"Kills: {amt}",
                med);
            killsScrollInner.AddChild(entry);
        }
    }
}