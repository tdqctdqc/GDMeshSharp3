
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeployOnLineOrder : UnitOrder
{
    public List<Vector2> FrontlinePoints { get; private set; }
    public List<Vector2> AdvanceLinePoints { get; private set; }
    public List<int> UnitIdsInLine { get; private set; }
    public bool GoThruHostile { get; private set; }
    public bool Attack { get; private set; }
    public DeployOnLineOrder(List<Vector2> frontlinePoints,
        List<Vector2> advanceLinePoints,
        List<int> unitIdsInLine, 
        bool goThruHostile,
        bool attack)
    {
        GoThruHostile = goThruHostile;
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
        var d = key.Data;
        var count = UnitIdsInLine.Count;
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        for (var i = 0; i < UnitIdsInLine.Count; i++)
        {
            var unit = d.Get<Unit>(UnitIdsInLine[i]);
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var pos = unit.Position.Copy();
            var movePoints = moveType.BaseSpeed;
            var target = FrontlinePoints.GetPointAlongLine(
                (v, w) => v.GetOffsetTo(w, d),
                (float)i / count);
            if (pos.Pos == target)
            {
                continue;
            }

            var moveCtx = new MoveData(unit.Id, moveType, movePoints, GoThruHostile, alliance);
            pos.MoveToPoint(moveCtx, target, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        }
    }

    public override void Draw(UnitGroup group, Vector2 relTo, 
        MeshBuilder mb, Data d)
    {
        var innerColor = group.Regime.Entity(d).PrimaryColor;
        var outerColor = group.Regime.Entity(d).PrimaryColor;
        var squareSize = 10f;
        var lineSize = 5f;
        var alliance = group.Regime.Entity(d).GetAlliance(d);
        for (var i = 0; i < FrontlinePoints.Count; i++)
        {
            var p = FrontlinePoints[i];
            var pRel = relTo.GetOffsetTo(p, d);
            if (i < FrontlinePoints.Count - 1)
            {
                var pNext = FrontlinePoints[i + 1];
                var pNextRel = relTo.GetOffsetTo(pNext, d);
                mb.AddLine(pRel, pNextRel, innerColor, lineSize);
            }
        }
        
        var count = UnitIdsInLine.Count();
        for (var i = 0; i < UnitIdsInLine.Count; i++)
        {
            var unit = d.Get<Unit>(UnitIdsInLine[i]);
            var pos = unit.Position;
            
            var target = FrontlinePoints.GetPointAlongLine(
                (v, w) => v.GetOffsetTo(w, d),
                (float)i / count);
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var startNode = new PointPathfindNode(pos.Pos, moveType, alliance, d);
            var endNode = new PointPathfindNode(target, moveType, alliance, d);
            PointPathfindNode.Join(startNode, endNode, d);
            var path = PathFinder.FindTacticalPath(
                startNode, 
                endNode,
                alliance,
                moveType,
                d
            );
            if (path != null)
            {
                for (var j = 0; j < path.Count - 1; j++)
                {
                    var from = path[j].Pos;
                    var to = path[j + 1].Pos;
                    mb.AddArrow(relTo.GetOffsetTo(from, d),
                        relTo.GetOffsetTo(to, d), 2f, Colors.Blue);
                }
            }
        }
    }
}