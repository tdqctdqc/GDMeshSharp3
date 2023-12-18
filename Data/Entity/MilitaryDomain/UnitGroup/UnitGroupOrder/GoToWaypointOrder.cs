
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
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        var context = d.Context;
        var currWp = g.GetWaypoint(d);
        var path = PathFinder.FindLandWaypointPath(currWp, destWp, alliance, d);
            // context.GetLandWaypointPath(currWp, destWp, alliance, d);
        if (path == null)
        {
            GD.Print($"couldnt find path from {currWp.Id} to {destWp.Id}");
            path = new List<Waypoint>();
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
            var currWp = context.UnitWaypoints[unit];
            var pos = unit.Position;
            var movePoints = 200f;
            bool onPath = true;
            if (PathWaypointIds.Contains(currWp.Id) == false)
            {
                var closestOnPath = path
                    .MinBy(wp => wp.Pos.GetOffsetTo(pos, d).Length());
                onPath = MoveTowards(ref pos, ref movePoints, closestOnPath, d);
                if (onPath) currWp = closestOnPath;
            }
            if (movePoints > 0 && onPath)
            {
                var index = PathWaypointIds.IndexOf(currWp.Id);
                while (movePoints > 0f && index < path.Count)
                {
                    currWp = path[index];
                    var prev = index > 0 ? path[index - 1] : null;
                    var reached = MoveTowards(ref pos, 
                        ref movePoints,
                        currWp, d, prev);
                    if (reached) index++;
                }
            }
            proc.NewUnitPosesById.Add(unit.Id, pos);
        }
    }

    private bool MoveTowards(ref Vector2 pos, ref float movePoints,
        Waypoint target, Data d, Waypoint from = null)
    {
        var roadMult = 1f;
        if (from != null)
        {
            var road = from.GetRoadWith(target, d);
            if (road != null) roadMult = road.SpeedMult;
        }
        var offset = pos.GetOffsetTo(target.Pos, d);
        var cost = offset.Length() / roadMult;
        if (cost > movePoints)
        {
            var ratio = movePoints / cost;
            pos = (pos + offset * ratio).ClampPosition(d);
            movePoints = 0;
            return false;
        }
        else
        {
            movePoints -= cost;
            pos = target.Pos;
            return true;
        }
    }
    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data data)
    {
        if (PathWaypointIds.Count == 0) return;

        var pos = group.GetPosition(data);
        var wps = PathWaypointIds
            .Select(id => MilitaryDomain.GetTacWaypoint(id, data));
        var close = wps
            .MinBy(wp => wp.Pos.GetOffsetTo(pos, data).Length());
        var index = PathWaypointIds.IndexOf(close.Id);
        mb.AddArrow(relTo.GetOffsetTo(pos, data),
            relTo.GetOffsetTo(close.Pos, data), 1f, Colors.Red);
        for (var i = index; i < PathWaypointIds.Count - 1; i++)
        {
            var from = MilitaryDomain.GetTacWaypoint(PathWaypointIds[i], data).Pos;
            var to = MilitaryDomain.GetTacWaypoint(PathWaypointIds[i + 1], data).Pos;
            mb.AddArrow(relTo.GetOffsetTo(from, data),
                relTo.GetOffsetTo(to, data), 1f, Colors.Red);
        }
    }
}