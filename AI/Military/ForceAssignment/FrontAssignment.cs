using System;
using System.Collections.Generic;
using System.Linq;
using GDMeshSharp3.Exception;
using Godot;

public class FrontAssignment : ForceAssignment
{
    public Front Front { get; private set; }

    public FrontAssignment(Front front) : base()
    {
        Front = front;
    }

    public override void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key)
    {
        var frontWps = Front.GetWaypoints(key.Data).ToHashSet();

        List<Waypoint> frontline = null;
        frontline = Front.GetFrontline(key.Data);
        Func<Unit, bool> distant = (u) =>
        {
            var wp = key.Data.Context.UnitWaypoints[u];
            return frontWps.Contains(wp);
        };
        foreach (var unitGroup in Groups)
        {
            UnitOrder order;
            var units = unitGroup.Units.Items(key.Data);
            if (units.Count() == 0)
            {
                order = new DoNothingUnitOrder();
            }
            else if (units.All(distant))
            {
                var wp = unitGroup.GetWaypoint(key.Data);
                if (wp == null) throw new Exception("missing unit waypoint");
                var close = frontWps
                    .OrderBy(fWp => key.Data.Planet.GetOffsetTo(fWp.Pos, wp.Pos).Length())
                    .First();
                order = new GoToWaypointOrder(close.Id);
                GD.Print("assigining go to waypoint");
            }
            else
            {
                var frontlineIds = frontline.Select(wp => wp.Id).ToList();
                order = new DeployOnLineOrder(frontlineIds);
            }
            key.SendMessage(new SetUnitOrderProcedure(unitGroup.MakeRef(), order));
        }
    }
}