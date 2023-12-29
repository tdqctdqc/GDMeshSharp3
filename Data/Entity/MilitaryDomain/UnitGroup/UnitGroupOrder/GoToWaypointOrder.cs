
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class GoToWaypointOrder : UnitOrder
{
    public List<int> PathWaypointIds { get; private set; }
    public static GoToWaypointOrder Construct(Waypoint destWp,
        Regime r, UnitGroup g, Data d)
    {
        var alliance = r.GetAlliance(d);
        var currWp = g.GetWaypoint(d);
        if (currWp == null) throw new Exception();
        var path = PathFinder.FindLandWaypointPath(currWp, destWp, alliance, d);
        if (path == null)
        {
            GD.Print($"{r.Name} couldn't find path from {currWp.Id} to {destWp.Id}");
            return null;
        }
        return new GoToWaypointOrder(path.Select(wp => wp.Id).ToList());
    }
    [SerializationConstructor] private GoToWaypointOrder(List<int> pathWaypointIds)
    {
        PathWaypointIds = pathWaypointIds;
    }
    
    public override void Handle(UnitGroup g, Data d, 
        HandleUnitOrdersProcedure proc)
    {
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        var context = d.Context;
        var path = PathWaypointIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d)).ToList();
        foreach (var unit in g.Units.Items(d))
        {
            var pos = unit.Position.Copy();
            var movePoints = Unit.MovePoints;
            unit.MoveOntoAndAlongPath(pos, ref movePoints, path, d);
            proc.NewUnitPosesById.Add(unit.Id, pos);
        }
    }

    

    
    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        if (PathWaypointIds.Count == 0) return;

        var pos = group.GetPosition(d);
        var wps = PathWaypointIds
            .Select(id => MilitaryDomain.GetTacWaypoint(id, d));
        var close = wps
            .MinBy(wp => wp.Pos.GetOffsetTo(pos, d).Length());
        var index = PathWaypointIds.IndexOf(close.Id);
        mb.AddArrow(relTo.GetOffsetTo(pos, d),
            relTo.GetOffsetTo(close.Pos, d), 1f, Colors.Red);
        for (var i = index; i < PathWaypointIds.Count - 1; i++)
        {
            var from = MilitaryDomain.GetTacWaypoint(PathWaypointIds[i], d).Pos;
            var to = MilitaryDomain.GetTacWaypoint(PathWaypointIds[i + 1], d).Pos;
            mb.AddArrow(relTo.GetOffsetTo(from, d),
                relTo.GetOffsetTo(to, d), 1f, Colors.Red);
        }
    }
}