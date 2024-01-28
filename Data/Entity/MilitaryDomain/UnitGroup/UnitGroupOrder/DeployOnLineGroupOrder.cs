
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DeployOnLineGroupOrder : UnitGroupOrder
{
    public List<FrontFace<PolyCell>> Faces { get; private set; }
    public bool Attack { get; private set; }
    public DeployOnLineGroupOrder(List<FrontFace<PolyCell>> faces,
        bool attack)
    {
        Faces = faces;
        Attack = attack;
    }

    public override void Handle(UnitGroup g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        var units = g.Units.Items(key.Data);
        var alliance = g.Regime.Entity(key.Data).GetAlliance(key.Data);
        var assgn = Assigner
            .PickBestAndAssignAlongFacesSingle<Unit, FrontFace<PolyCell>>
        (
            Faces,
            units,
            u => u.GetPowerPoints(key.Data),
            (u, f) => 
                u.Position.GetCell(key.Data).GetCenter().GetOffsetTo(
                    PlanetDomainExt.GetPolyCell(f.Native, key.Data).GetCenter(), key.Data).Length(),
            
            units.Sum(u => u.GetPowerPoints(key.Data)),
            f =>
            {
                return 1f;
                var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, key.Data);
                if (foreignCell.Controller.RefId == -1) return 0f;
                var foreignRegime = foreignCell.Controller.Entity(key.Data);
                var foreignAlliance = foreignRegime.GetAlliance(key.Data);
                var units = foreignCell.GetUnits(key.Data);
                if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;
                if (alliance.Rivals.Contains(foreignAlliance) == false) return 0f;
                float mult = FrontAssignment.DesiredOpposingPpRatio;
                return units.Sum(u => u.GetPowerPoints(key.Data)) * mult;
            }
        );
        foreach (var unit in units)
        {
            var dest = PlanetDomainExt.GetPolyCell(assgn[unit].Native, key.Data);
            var pos = unit.Position.Copy();
            var moveType = unit.Template.Entity(key.Data).MoveType.Model(key.Data);
            var movePoints = moveType.BaseSpeed;
            var moveCtx = new MoveData(unit.Id, moveType, movePoints, alliance);
            pos.MoveToCell(moveCtx, dest, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        }
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

    public override void RegisterCombatActions(CombatCalculator combat, LogicWriteKey key)
    {
        // if (Attack == false) return null;
        // if (AdvanceLinePoints == null) return null;
        // var res = new KeyValuePair<Unit, CombatAction>[UnitIdsInLine.Count];
        // for (var i = 0; i < UnitIdsInLine.Count; i++)
        // {
        //     var unit = d.Get<Unit>(UnitIdsInLine[i]);
        //     var target = AdvanceLinePoints.GetPointAlongLine(
        //         (v, w) => v.GetOffsetTo(w, d),
        //         (float)i / UnitIdsInLine.Count);
        //     var action = new LandAttackAction(target.ClampPosition(d));
        //     res[i] = new KeyValuePair<Unit, CombatAction>(unit, action);
        // }
        //
        // return res;
    }
}