using System.Linq;
using Godot;

namespace Ui.Combat;

public partial class GeneralTab : HBoxContainer, IUiDrawable
{
    private CombatInfo _info;
    public GeneralTab(CombatInfo info)
    {
        _info = info;
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

        res.CreateLabelAsChild($"Landform: {_info.Landform}");
        res.CreateLabelAsChild($"Vegetation: {_info.Vegetation}");
        res.CreateLabelAsChild($"Attackers {(_info.DefendersForcedBack ? "victorious" : "defeated")}");
        
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
            ? _info.Attackers
            : _info.Defenders;
        
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