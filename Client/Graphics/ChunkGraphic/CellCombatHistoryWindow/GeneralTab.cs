using System.Linq;
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
        res.ExpandFill();

        var history = Parent.Info;
        var cell = PlanetDomainExt.GetPolyCell(history.Cell.RefId, client.Data);
        res.CreateLabelAsChild($"Cell: {cell.Id}");
        res.CreateLabelAsChild($"Landform: {cell.Landform.Get(client.Data).Name}");
        res.CreateLabelAsChild($"Vegetation: {cell.Vegetation.Get(client.Data).Name}");
        res.CreateLabelAsChild($"Attackers {(history.DefendersForcedBack ? "victorious" : "defeated")}");
        
        return res;
    }

    private VBoxContainer Losses(bool attacker, 
        Client client)
    {
        var res = new VBoxContainer();
        res.CreateLabelAsChild($"{(attacker ? "Attacker " : "Defender ")} Losses");
        var scroll = new ScrollContainer();
        res.ExpandFill();
        scroll.ExpandFill();
        var scrollInner = new VBoxContainer();
        scroll.AddChild(scrollInner);
        res.AddChild(scroll);
        var infos = attacker 
            ? Parent.Graph.GetNeighbors(Parent.Info)
                .OfType<CellAttackNode>().SelectMany(n => n.UnitInfos)
            : Parent.Info.UnitInfos;
        
        var lossesSum = IdCount<Troop>.Sum(infos.Select(i => i.GetLosses()).ToArray());
        
        var troopsSum = IdCount<Troop>.Sum(infos.Select(i => i.Initial).ToArray());
        
        var iconSize = client.Settings.SmallIconSize.Value;
        var e = troopsSum
            .GetEnumModel(client.Data);
        foreach (var (troop, amt) in e)
        {
            var entry = troop.Icon.GetLabeledIcon<HBoxContainer>(
                $"Deployed: {amt} Losses: {lossesSum.Get(troop)}",
                iconSize);
            scrollInner.AddChild(entry);
        }

        return res;
    }
}