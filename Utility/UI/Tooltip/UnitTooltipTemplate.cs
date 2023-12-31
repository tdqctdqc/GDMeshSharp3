

using System;
using System.Collections.Generic;
using Godot;

public class UnitTooltipTemplate : TooltipTemplate<Unit>
{
    protected override List<Func<Unit, Data, Control>> _fastGetters { get; }
        = new()
        {
            GetRegimeEtc,
            GetTroopCounts
        };
    protected override List<Func<Unit, Data, Control>> _slowGetters { get; }
        = new()
        {
        };

    private static Control GetRegimeEtc(Unit u, Data d)
    {
        var r = u.Regime.Entity(d);
        return NodeExt.CreateLabel($"{r.Name} \n{u.Id}");
    }
    private static Control GetTroopCounts(Unit u, Data d)
    {
        var box = new VBoxContainer();
        var iconSize = Game.I.Client.Settings.MedIconSize.Value;
        foreach (var kvp in u.Troops.Contents)
        {
            var num = kvp.Value;
            var troop = d.Models.GetModel<Troop>(kvp.Key);
            var labeled = troop.Icon.GetLabeledIcon<HBoxContainer>(
                num.ToString(), iconSize);
            box.AddChild(labeled);
        }

        return box;
    }
    
}