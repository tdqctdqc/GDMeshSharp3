
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class GoToCellGroupOrder : UnitGroupOrder
{
    public List<int> PathWaypointIds { get; private set; }
    public static GoToCellGroupOrder Construct(PolyCell destWp,
        Regime r, UnitGroup g, Data d)
    {
        var alliance = r.GetAlliance(d);
        var currWp = g.GetCell(d);
        var moveType = g.MoveType(d);
        
        if (moveType.Passable(destWp, alliance, d) == false)
        {
            throw new Exception($"{moveType.GetType().Name} cant go to {destWp.GetType().Name}" +
                                $" alliance {alliance.Leader.Entity(d).Id}" +
                                $" occupier {destWp.Controller.RefId} ");
        }
        var path = PathFinder
            .FindPath(moveType, alliance, 
                currWp, destWp, d);
        if (path == null)
        {
            var issue = new CantFindPathIssue(currWp.GetCenter(),
                alliance, $"failed to find path",
                currWp, destWp, moveType);
            d.ClientPlayerData.Issues.Add(issue);
            return null;
        }
        
        return new GoToCellGroupOrder(path.Select(wp => wp.Id).ToList());
    }
    [SerializationConstructor] private GoToCellGroupOrder(List<int> pathWaypointIds)
    {
        PathWaypointIds = pathWaypointIds;
    }
    
    public override void Handle(UnitGroup g, LogicWriteKey key, 
        HandleUnitOrdersProcedure proc)
    {
        var d = key.Data;
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        var context = d.Context;
        var path = PathWaypointIds.Select(id => PlanetDomainExt.GetPolyCell(id, d)).ToList();
        foreach (var unit in g.Units.Items(d))
        {
            var pos = unit.Position.Copy();
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var movePoints = moveType.BaseSpeed;
            var ctx = new MoveData(unit.Id, moveType, movePoints, alliance);
            pos.MoveOntoAndAlongStrategicPath(ctx, path, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        }
    }

    

    
    public override void Draw(UnitGroup group, Vector2 relTo, MeshBuilder mb, Data d)
    {
        if (PathWaypointIds.Count == 0) return;

        var pos = group.GetPosition(d);
        var wps = PathWaypointIds
            .Select(id => PlanetDomainExt.GetPolyCell(id, d));
        var close = wps
            .MinBy(wp => wp.GetCenter().GetOffsetTo(pos, d).Length());
        var index = PathWaypointIds.IndexOf(close.Id);
        mb.AddArrow(relTo.GetOffsetTo(pos, d),
            relTo.GetOffsetTo(close.GetCenter(), d), 1f, Colors.Red);
        for (var i = index; i < PathWaypointIds.Count - 1; i++)
        {
            var from = PlanetDomainExt.GetPolyCell(PathWaypointIds[i], d).GetCenter();
            var to = PlanetDomainExt.GetPolyCell(PathWaypointIds[i + 1], d).GetCenter();
            mb.AddArrow(relTo.GetOffsetTo(from, d),
                relTo.GetOffsetTo(to, d), 5f, Colors.Pink);
        }
    }

    public override CombatResult[] GetCombatResults(UnitGroup g, CombatCalculator.CombatCalcData cData, Data d)
    {
        return this.DefaultCombatResults(g, cData, d);
    }
}