
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
        var moveType = g.MoveType(d);
        
        if (moveType.Passable(destWp, alliance, false, d) == false)
        {
            throw new Exception($"{moveType.GetType().Name} cant go to {destWp.GetType().Name}" +
                                $" alliance {alliance.Leader.Entity(d).Id}" +
                                $" occupier {destWp.GetOccupyingRegime(d)?.GetAlliance(d).Leader.Entity(d).Id} ");
        }
        
        
        var path = PathFinder
            .FindPath(moveType, alliance, currWp, destWp, false, d);
        if (path == null)
        {
            var issue = new CantFindWaypointPathIssue(currWp.Pos,
                alliance, $"failed to find path",
                currWp, destWp, moveType, false);
            d.ClientPlayerData.Issues.Add(issue);
            return null;
        }
        for (var i = 0; i < path.Count; i++)
        {
            if (moveType.Passable(path[i], alliance, false, d) == false)
            {
                var issue = new CantFindWaypointPathIssue(path[i].Pos,
                    alliance, $"impassable at {i + 1} / {path.Count}",
                    currWp, destWp, moveType, false);
                d.ClientPlayerData.Issues.Add(issue);
                return null;
            }
        }
        
        return new GoToWaypointOrder(path.Select(wp => wp.Id).ToList());
    }
    [SerializationConstructor] private GoToWaypointOrder(List<int> pathWaypointIds)
    {
        PathWaypointIds = pathWaypointIds;
    }
    
    public override void Handle(UnitGroup g, LogicWriteKey key, 
        HandleUnitOrdersProcedure proc)
    {
        var d = key.Data;
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        var context = d.Context;
        var path = PathWaypointIds.Select(id => MilitaryDomain.GetTacWaypoint(id, d)).ToList();
        foreach (var unit in g.Units.Items(d))
        {
            var pos = unit.Position.Copy();
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var movePoints = moveType.BaseSpeed;
            var ctx = new Mover.MoveData(unit.Id, moveType, movePoints, false, alliance);
            pos.MoveOntoAndAlongPath(ctx, path, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
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
                relTo.GetOffsetTo(to, d), 5f, Colors.Pink);
        }
    }
}