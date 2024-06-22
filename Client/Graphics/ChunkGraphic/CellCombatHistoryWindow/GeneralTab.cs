using Godot;

namespace Ui.CellCombatHistoryWindow;

public partial class GeneralTab : HBoxContainer, IUiDrawable
{
    public global::CellCombatHistoryWindow Parent { get; private set; }

    public GeneralTab(global::CellCombatHistoryWindow parent)
    {
        Parent = parent;
        Name = "General";
    }
    private GeneralTab()
    {
    }
    

    public void Draw(Client c)
    {
        this.ClearChildren();
        AddChild(Info(c));
        AddChild(Losses(true, c));
        AddChild(Losses(false, c));
    }

    private VBoxContainer Info(Client client)
    {
        var res = new VBoxContainer();
        res.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        res.SizeFlagsVertical = Control.SizeFlags.ExpandFill;

        var history = Parent.History;
        var cell = PlanetDomainExt.GetPolyCell(history.CellId, client.Data);
        res.CreateLabelAsChild($"Cell: {cell.Id}");
        res.CreateLabelAsChild($"Landform: {cell.Landform.Get(client.Data).Name}");
        res.CreateLabelAsChild($"Vegetation: {cell.Vegetation.Get(client.Data).Name}");
        res.CreateLabelAsChild($"Attackers {(history.ForcedBack ? "victorious" : "defeated")}");
        
        return res;
    }

    private VBoxContainer Losses(bool attacker, Client client)
    {
        var history = Parent.History;
        var res = new VBoxContainer();
        res.CreateLabelAsChild($"{(attacker ? "Attacker " : "Defender ")} Losses");
        var scroll = new ScrollContainer();
        res.SizeFlagsVertical = SizeFlags.ExpandFill;
        res.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var scrollInner = new VBoxContainer();
        scroll.AddChild(scrollInner);
        res.AddChild(scroll);
        var troops = attacker 
            ? history.AttackerTroops 
            : history.DefenderTroops;

        var losses = attacker 
            ? history.AttackerLosses 
            : history.DefenderLosses;
        var lossesSum = IdCount<Troop>.Sum(losses);

        var troopsSum = IdCount<Troop>.Sum(troops);


        var iconSize = client.Settings.SmallIconSize.Value;
        var e = troopsSum.GetEnumerableModel(client.Data);
        foreach (var (troop, amt) in e)
        {
            var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"Losses: {lossesSum.Get(troop)} / {amt}",
                iconSize);
            scrollInner.AddChild(entry);
        }

        return res;
    }
}