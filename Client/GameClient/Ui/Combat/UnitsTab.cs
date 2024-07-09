using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class UnitsTab : HBoxContainer, IUiDrawable
{
    private CombatInfo _info;
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
        var attackersList = new ItemListToken<UnitCombatInfo>(
            atkInfos,
            u =>
            {
                return u.Template.Fulfilled()
                    ? u.Template.Get(c.Data).Name + " " + u.Unit
                    : "None";
            },
            u => DrawInfo(u, true, info, c)
        );
        attackers.AddChild(attackersList.ItemList);
        attackersList.ItemList.ExpandFill();

        
        var defenders = new VBoxContainer();
        defenders.ExpandFill();
        defenders.CreateLabelAsChild("Defenders");
        var defendersList = new ItemListToken<UnitCombatInfo>(
            _info.Defenders,
            u =>
            {
                return u.Template.Fulfilled()
                    ? u.Template.Get(c.Data).Name + " " + u.Unit
                    : "None";
            },
            u => DrawInfo(u, false, info, c)
        );
        defendersList.ItemList.ExpandFill();
        defenders.AddChild(defendersList.ItemList);
        scrolls.AddChild(attackers);
        scrolls.AddChild(defenders);
        AddChild(scrolls);
        AddChild(info);
    }

    private void DrawInfo(UnitCombatInfo u, bool attacker,
        Control info,
        Client c)
    {
        info.ClearChildren();
        
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