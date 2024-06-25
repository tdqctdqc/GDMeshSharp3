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
        
        var attackers = new VBoxContainer();
        attackers.ExpandFill();
        attackers.CreateLabelAsChild("Attackers");
        // var attackersList = new ItemListToken<Unit>(
        //     Parent.Info.AttackerUnits.Select(u => u.Get(c.Data)),
        //     u => u.Template.Get(c.Data).Name + " " + u.Id,
        //     u => DrawInfo(u, true, info, c),
        //     size
        // );
        // attackers.AddChild(attackersList.ItemList);
        // attackersList.ItemList.ExpandFill();

        
        var defenders = new VBoxContainer();
        defenders.ExpandFill();
        defenders.CreateLabelAsChild("Defenders");
        // var defendersList = new ItemListToken<Unit>(
        //     Parent.Info.DefenderUnits.Select(u => u.Get(c.Data)),
        //     u => u.Template.Get(c.Data).Name + " " + u.Id,
        //     u => DrawInfo(u, false, info, c),
        //     size
        // );
        // defendersList.ItemList.ExpandFill();
        // defenders.AddChild(defendersList.ItemList);
        scrolls.AddChild(attackers);
        scrolls.AddChild(defenders);
        AddChild(scrolls);
        AddChild(info);
    }

    private void DrawInfo(Unit u, bool attacker,
        Control info,
        Client c)
    {
        info.ClearChildren();
        
        var large = c.Settings.LargeIconSize.Value;
        var med = c.Settings.MedIconSize.Value;

        var icon = u.GetMaxPowerTroop(c.Data).Icon
            .GetLabeledIcon<HBoxContainer>(
                $"{u.Template.Get(c.Data).Name} {u.Id}",
                large);
        info.AddChild(icon);
        info.CreateLabelAsChild($"{(attacker ? "Attacker" : "Defender")}");
        
        // var index = attacker
        //     ? Parent.Info.AttackerUnits.IndexOf(u.MakeRef())
        //     : Parent.Info.DefenderUnits.IndexOf(u.MakeRef());
        // var troops = attacker
        //     ? Parent.Info.AttackerTroops[index]
        //     : Parent.Info.DefenderTroops[index];
        // var losses = attacker
        //     ? Parent.Info.AttackerLosses[index]
        //     : Parent.Info.DefenderLosses[index];
        // var kills = attacker
        //     ? Parent.Info.AttackerKills[index]
        //     : Parent.Info.DefenderKills[index];
        //
        // info.CreateLabelAsChild("Troops engaged and lost");
        //
        // var troopsScroll = new ScrollContainer();
        // troopsScroll.CustomMinimumSize = Vector2.One * 200f;
        // troopsScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        // var troopsScrollInner = new VBoxContainer();
        // troopsScrollInner.ExpandFill();
        // info.AddChild(troopsScrollInner);
        // foreach (var (troop, amt) in troops.GetEnumerableModel(c.Data))
        // {
        //     var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
        //         $"Losses: {losses.Get(troop)} / {amt}",
        //         med);
        //     troopsScrollInner.AddChild(entry);
        // }
        //
        // info.CreateLabelAsChild("Troops killed");
        // var killsScroll = new ScrollContainer();
        // killsScroll.CustomMinimumSize = Vector2.One * 200f;
        // killsScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        // var killsScrollInner = new VBoxContainer();
        // killsScrollInner.ExpandFill();
        // info.AddChild(killsScrollInner);
        // foreach (var (troop, amt) in kills.GetEnumerableModel(c.Data))
        // {
        //     var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
        //         $"Kills: {amt}",
        //         med);
        //     killsScrollInner.AddChild(entry);
        // }
    }
}