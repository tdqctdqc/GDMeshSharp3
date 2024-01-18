
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeployOnLineGroupOrder : UnitGroupOrder, ICombatOrder
{
    public int RallyWaypointId { get; private set; }
    public List<Vector2> FrontlinePoints { get; private set; }
    public List<Vector2> AdvanceLinePoints { get; private set; }
    public List<int> UnitIdsInLine { get; private set; }
    public bool Attack { get; private set; }
    public DeployOnLineGroupOrder(List<Vector2> frontlinePoints,
        List<Vector2> advanceLinePoints,
        List<int> unitIdsInLine, 
        bool attack,
        int rallyWaypointId)
    {
        RallyWaypointId = rallyWaypointId;
        FrontlinePoints = frontlinePoints;
        AdvanceLinePoints = advanceLinePoints;
        UnitIdsInLine = unitIdsInLine;
        Attack = attack;
        for (var i = 0; i < frontlinePoints.Count; i++)
        {
            var p = frontlinePoints[i];
            if (float.IsNaN(p.X) || float.IsNaN(p.Y))
            {
                throw new Exception("bad point " + p);
            }
        }
    }

    public override void Handle(UnitGroup g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        // var d = key.Data;
        // var count = UnitIdsInLine.Count;
        // var alliance = g.Regime.Entity(d).GetAlliance(d);
        // for (var i = 0; i < UnitIdsInLine.Count; i++)
        // {
        //     var unit = d.Get<Unit>(UnitIdsInLine[i]);
        //     var moveType = unit.Template.Entity(d).MoveType.Model(d);
        //     var pos = unit.Position.Copy();
        //     var movePoints = moveType.BaseSpeed;
        //     var frontLinePosition = GetFrontLinePos(unit, d);
        //     var moveCtx = new MoveData(unit.Id, moveType, movePoints, alliance);
        //     if (AdvanceLinePoints == null)
        //     {
        //         if (pos.Pos == frontLinePosition)
        //         {
        //             continue;
        //         }
        //         pos.MoveToPoint(moveCtx, frontLinePosition, key);
        //         proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        //         continue;
        //     }
        //
        //     var advanceLinePosition = GetAdvanceLinePos(unit, d);
        //     var closeOffset = Vector2Ext
        //         .GetClosestPointOnLineSegment(
        //             Vector2.Zero,
        //             pos.Pos.GetOffsetTo(frontLinePosition, d),
        //             pos.Pos.GetOffsetTo(advanceLinePosition, d)
        //         );
        //     
        //     var closeEnemies = d.Military.UnitAux.UnitGrid
        //         .GetWithin(unit.Position.Pos, closeOffset.Length(),
        //             e => e.Hostile(alliance, d));
        //     Vector2 target;
        //     if (closeEnemies.Count() > 0)
        //     {
        //         var closeEnemyDist = closeEnemies
        //             .Min(e => unit.Position.Pos.GetOffsetTo(e.Position.Pos, d).Length());
        //         var dist = Mathf.Max(0f, closeEnemyDist - unit.Radius() * 2f);
        //         target = pos.Pos + closeOffset.Normalized() * closeEnemyDist;
        //     }
        //     else
        //     {
        //         target = pos.Pos + closeOffset;
        //     }
        //     target = target.ClampPosition(d);
        //     pos.MoveToPoint(moveCtx, target, key);
        //     proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        // }
    }

    public override void Draw(UnitGroup group, Vector2 relTo, 
        MeshBuilder mb, Data d)
    {
        // var innerColor = group.Regime.Entity(d).PrimaryColor;
        // var outerColor = group.Regime.Entity(d).PrimaryColor;
        // var squareSize = 10f;
        // var lineSize = 5f;
        // var alliance = group.Regime.Entity(d).GetAlliance(d);
        // for (var i = 0; i < FrontlinePoints.Count; i++)
        // {
        //     var p = FrontlinePoints[i];
        //     var pRel = relTo.GetOffsetTo(p, d);
        //     if (i < FrontlinePoints.Count - 1)
        //     {
        //         var pNext = FrontlinePoints[i + 1];
        //         var pNextRel = relTo.GetOffsetTo(pNext, d);
        //         mb.AddLine(pRel, pNextRel, innerColor, lineSize);
        //     }
        // }
        //
        // var count = UnitIdsInLine.Count();
        // for (var i = 0; i < UnitIdsInLine.Count; i++)
        // {
        //     var unit = d.Get<Unit>(UnitIdsInLine[i]);
        //     var pos = unit.Position;
        //     
        //     var target = FrontlinePoints.GetPointAlongLine(
        //         (v, w) => v.GetOffsetTo(w, d),
        //         (float)i / count);
        //     var moveType = unit.Template.Entity(d).MoveType.Model(d);
        //     var startNode = new PointPathfindNode(pos.Pos, moveType, alliance, d);
        //     var endNode = new PointPathfindNode(target, moveType, alliance, d);
        //     PointPathfindNode.Join(startNode, endNode, d);
        //     var path = PathFinder.FindTacticalPath(
        //         startNode, 
        //         endNode,
        //         alliance,
        //         moveType,
        //         d
        //     );
        //     if (path != null)
        //     {
        //         for (var j = 0; j < path.Count - 1; j++)
        //         {
        //             var from = path[j].Pos;
        //             var to = path[j + 1].Pos;
        //             mb.AddArrow(relTo.GetOffsetTo(from, d),
        //                 relTo.GetOffsetTo(to, d), 2f, Colors.Blue);
        //         }
        //     }
        // }
    }

    public override CombatResult[] GetCombatResults(
        UnitGroup g, CombatCalculator.CombatCalcData cData, 
        Data d)
    {
        var results = this.InitializeResultsWithLosses(
            g, cData, d);
        if (results.Length == 0) return results;

        return results;
        
        //
        //
        // var rallyWp = MilitaryDomain.GetWaypoint(RallyWaypointId, d);
        // var advanceDist = 5f;
        // var retreatDist = 10f;
        // foreach (var result in results)
        // {
        //     var lineIndex = UnitIdsInLine.IndexOf(result.Unit.RefId);
        //     if (lineIndex == -1) continue;
        //     var unit = result.Unit.Entity(d);
        //     var frontLinePos = GetFrontLinePos(unit, d);
        //     Vector2 axis;
        //     if (AdvanceLinePoints != null)
        //     {
        //         var advanceLinePos = GetAdvanceLinePos(unit, d);
        //         axis = unit.Position.Pos
        //             .GetOffsetTo(advanceLinePos, d);
        //     }
        //     else
        //     {
        //         axis = rallyWp.Pos
        //             .GetOffsetTo(unit.Position.Pos, d);
        //     }
        //     if (result.HeldPosition(cData, d) == false)
        //     {
        //         retreatDist = Mathf.Min(retreatDist, axis.Length());
        //         result.ResultOffset = -axis.Normalized() * retreatDist;
        //     }
        //     else if (result.SuccessfulAttack(cData, d))
        //     {
        //         advanceDist = Mathf.Min(advanceDist, axis.Length());
        //         result.ResultOffset = axis.Normalized() * advanceDist;
        //     }
        // }
        //
        // return results;
    }

    private Vector2 GetFrontLinePos(Unit u, Data d)
    {
        var i = UnitIdsInLine.IndexOf(u.Id);
        if (i == -1) throw new Exception();
        var alongLineRatio = (float)i / UnitIdsInLine.Count;
        return FrontlinePoints.GetPointAlongLine(
            (v, w) => v.GetOffsetTo(w, d),
            alongLineRatio);
    }

    private Vector2 GetAdvanceLinePos(Unit u, Data d)
    {
        if (AdvanceLinePoints == null) throw new Exception();
        var i = UnitIdsInLine.IndexOf(u.Id);
        if (i == -1) throw new Exception();
        var alongLineRatio = (float)i / UnitIdsInLine.Count;
        return AdvanceLinePoints.GetPointAlongLine(
            (v, w) => v.GetOffsetTo(w, d),
            alongLineRatio);
    }
    public KeyValuePair<Unit, CombatAction>[] DecideCombatAction(Data d)
    {
        if (Attack == false) return null;
        if (AdvanceLinePoints == null) return null;
        var res = new KeyValuePair<Unit, CombatAction>[UnitIdsInLine.Count];
        for (var i = 0; i < UnitIdsInLine.Count; i++)
        {
            var unit = d.Get<Unit>(UnitIdsInLine[i]);
            var target = AdvanceLinePoints.GetPointAlongLine(
                (v, w) => v.GetOffsetTo(w, d),
                (float)i / UnitIdsInLine.Count);
            var action = new LandAttackAction(target.ClampPosition(d));
            res[i] = new KeyValuePair<Unit, CombatAction>(unit, action);
        }

        return res;
    }
}