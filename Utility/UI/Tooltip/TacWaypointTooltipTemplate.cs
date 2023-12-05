
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TacWaypointTooltipTemplate : TooltipTemplate<Waypoint>
{
    protected override List<Func<Waypoint, Data, Control>> _fastGetters { get; }
        = new ()
        {
            GetControllingAlliances,
            GetResponsibleRegimes,
            GetOccupier
        };

    private static Control GetOccupier(Waypoint wp, Data d)
    {
        var occupier = wp.GetOccupyingRegime(d);
        if (occupier != null)
        {
            return NodeExt.CreateLabel("Occupier: " + occupier.Name);
        }

        return new Control();
    }

    protected override List<Func<Waypoint, Data, Control>> _slowGetters { get; }
        = new ();



    private static Control GetControllingAlliances(Waypoint wp, Data d)
    {
        var alliances = wp.GetForceBalance(d).GetControllingAlliances();
        return NodeExt.MakeLabelList(alliances.Select(a => a.Leader.Entity(d).Name), "Controlling Alliances");
    }

    private static Control GetResponsibleRegimes(Waypoint wp, Data d)
    {
        var regimes = d.HostLogicData.AllianceAis.Dic.Values
            .SelectMany(v => v.MilitaryAi.AreasOfResponsibility
                .Where(kvp =>
                    kvp.Value.Contains(wp)))
            .Select(kvp => kvp.Key);
        return NodeExt.MakeLabelList(regimes.Select(r => r.Name), "Responsible Regimes");

    }

}