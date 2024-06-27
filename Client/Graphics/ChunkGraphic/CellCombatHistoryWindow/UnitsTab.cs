using System.Linq;
using Godot;

namespace Ui.CellCombatHistoryWindow;

public partial class UnitsTab : HBoxContainer, IUiDrawable
{
    public global::CellCombatHistoryWindow Parent { get; private set; }
    public UnitsTab(global::CellCombatHistoryWindow parent)
    {
        Parent = parent;
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

        var atkInfos = Parent.Graph.GetNeighbors(Parent.Info)
            .OfType<CellAttackNode>().SelectMany(n => n.UnitInfos);
        
        var attackers = new VBoxContainer();
        attackers.ExpandFill();
        attackers.CreateLabelAsChild("Attackers");
        var attackersList = new ItemListToken<UnitCombatInfo>(
            atkInfos,
            u => u.Template.Get(c.Data).Name + " " + u.Id,
            u => DrawInfo(u, true, info, c),
            size
        );
        attackers.AddChild(attackersList.ItemList);
        attackersList.ItemList.ExpandFill();

        
        var defenders = new VBoxContainer();
        defenders.ExpandFill();
        defenders.CreateLabelAsChild("Defenders");
        var defendersList = new ItemListToken<UnitCombatInfo>(
            Parent.Info.UnitInfos,
            u => u.Template.Get(c.Data).Name + " " + u.Id,
            u => DrawInfo(u, false, info, c),
            size
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
        var icon = template.GetMaxPowerTroop(c.Data).Icon
            .GetLabeledIcon<HBoxContainer>(
                $"{template.Name} {u.Id}",
                large);
        info.AddChild(icon);
        info.CreateLabelAsChild($"{(attacker ? "Attacker" : "Defender")}");
        
        var losses = u.GetLosses();
        
        info.CreateLabelAsChild("Troops engaged and lost");
        
        var troopsScroll = new ScrollContainer();
        troopsScroll.CustomMinimumSize = Vector2.One * 200f;
        troopsScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        var troopsScrollInner = new VBoxContainer();
        troopsScrollInner.ExpandFill();
        info.AddChild(troopsScrollInner);
        foreach (var (troop, amt) in u.Initial.GetEnumModel(c.Data))
        {
            var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"Deployed: {amt} Losses: {losses.Get(troop)}",
                med);
            troopsScrollInner.AddChild(entry);
        }
        
        info.CreateLabelAsChild("Troops killed");
        var killsScroll = new ScrollContainer();
        killsScroll.CustomMinimumSize = Vector2.One * 200f;
        killsScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        var killsScrollInner = new VBoxContainer();
        killsScrollInner.ExpandFill();
        info.AddChild(killsScrollInner);
        foreach (var (troop, amt) in u.Kills.GetEnumModel(c.Data))
        {
            var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"Kills: {amt}",
                med);
            killsScrollInner.AddChild(entry);
        }
    }
}