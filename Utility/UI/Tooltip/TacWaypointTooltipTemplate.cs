
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TacWaypointTooltipTemplate : TooltipTemplate<Waypoint>
{
    protected override List<Func<Waypoint, Data, Control>> _fastGetters { get; }
        = new ()
        {
            GetOccupier,
            GetTheaters,
            // GetControllingAlliances,
            // GetResponsibleRegimes,
        };

    private static Control GetTheaters(Waypoint wp, Data d)
    {
        var regimes = d.HostLogicData.AllianceAis.Dic.Values
            .SelectMany(v => v.MilitaryAi.AreasOfResponsibility
                .Where(kvp =>
                    kvp.Value.Contains(wp)))
            .Select(kvp => kvp.Key);
        var res = "";
        foreach (var regime in regimes)
        {
            if (d.HostLogicData.RegimeAis.Dic.ContainsKey(regime) == false) continue;
            var ai = d.HostLogicData.RegimeAis[regime];
            var theater = ai.Military.Deployment.ForceAssignments.SelectWhereOfType<TheaterAssignment>()
                .FirstOrDefault(t => t.TacWaypointIds.Contains(wp.Id));
            if (theater == null) continue;
            res += $"\n{regime.Name} Theater: " + theater.Id;
            var front = theater.Fronts.FirstOrDefault(f => f.TacWaypointIds.Contains(wp.Id));
            if (front == null) continue;
            res += "\n  Front: " + front.Id;
            var seg = front.Segments.FirstOrDefault(s => s.TacWaypointIds.Contains(wp.Id));
            if (seg == null) continue;
            res += "\n    Segment: " + seg.Id;
            res += "\n    Groups: " + seg.Groups(d).Count();
        }

        if (res == "") return new Control();
        return NodeExt.CreateLabel(res);
    }
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