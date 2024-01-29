
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
        var assgn = GetAssignments(g, key.Data);
        foreach (var unit in units)
        {
            var dest = PlanetDomainExt
                .GetPolyCell(assgn[unit].Native, key.Data);
            var pos = unit.Position.Copy();
            var moveType = unit.Template.Entity(key.Data).MoveType.Model(key.Data);
            var movePoints = moveType.BaseSpeed;
            var moveCtx = new MoveData(unit.Id, moveType, movePoints, alliance);
            pos.MoveToCell(moveCtx, dest, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        }
    }

    private Dictionary<Unit, FrontFace<PolyCell>> GetAssignments(UnitGroup g, Data d)
    {
        var units = g.Units.Items(d);
        var alliance = g.Regime.Entity(d).GetAlliance(d);
        return Assigner
            .PickBestAndAssignAlongFacesSingle<Unit, FrontFace<PolyCell>>
            (
                Faces,
                units,
                u => u.GetPowerPoints(d),
                (u, f) => 
                    u.Position.GetCell(d).GetCenter().GetOffsetTo(
                        PlanetDomainExt.GetPolyCell(f.Native, d).GetCenter(), d).Length(),
                units.Sum(u => u.GetPowerPoints(d)),
                f =>
                {
                    return 1f;
                    var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
                    if (foreignCell.Controller.RefId == -1) return 0f;
                    var foreignRegime = foreignCell.Controller.Entity(d);
                    var foreignAlliance = foreignRegime.GetAlliance(d);
                    var units = foreignCell.GetUnits(d);
                    if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;
                    if (alliance.IsRivals(foreignAlliance, d) == false) return 0f;
                    float mult = FrontAssignment.DesiredOpposingPpRatio;
                    return units.Sum(u => u.GetPowerPoints(d)) * mult;
                }
            );
    }
    public override void Draw(UnitGroup group, Vector2 relTo, 
        MeshBuilder mb, Data d)
    {
        var innerColor = group.Regime.Entity(d).PrimaryColor;
        var outerColor = group.Regime.Entity(d).PrimaryColor;
        var squareSize = 10f;
        var lineSize = 5f;
        var alliance = group.Regime.Entity(d).GetAlliance(d);
        var assgns = GetAssignments(group, d);



        var natives = Faces.Select(f => f.GetNative(d)).Distinct();
        var foreigns = Faces.Select(f => f.GetForeign(d)).Distinct();
        foreach (var n in natives)
        {
            mb.DrawPolygon(n.RelBoundary.Select(p => relTo.GetOffsetTo(p + n.RelTo, d)).ToArray(),
                new Color(Colors.Blue, .5f));
        }
        foreach (var n in foreigns)
        {
            mb.DrawPolygon(n.RelBoundary.Select(p => relTo.GetOffsetTo(p + n.RelTo, d)).ToArray(),
                new Color(Colors.Red, .5f));
        }
        
        for (var i = 0; i < Faces.Count; i++)
        {
            var face = Faces[i];
            var native = face.GetNative(d);
            var foreign = face.GetForeign(d);
            // var close = native.RelBoundary
            //     .OrderBy(p =>
            //     {
            //         var minDist = Mathf.Inf;
            //         for (var j = 0; j < foreign.RelBoundary.Length; j++)
            //         {
            //             var fFrom = native.RelTo.GetOffsetTo(foreign.RelBoundary[j] + foreign.RelTo, d);
            //             var fTo = native.RelTo.GetOffsetTo(foreign.RelBoundary.Modulo(j + 1) + foreign.RelTo, d);;
            //             var minP = Geometry2D.GetClosestPointToSegment(p, fFrom, fTo);
            //             minDist = Mathf.Min(minDist, p.DistanceTo(minP));
            //         }
            //
            //         return minDist;
            //     }).Take(2);
            // mb.AddLine(relTo.GetOffsetTo(close.First() + native.RelTo, d),
            //     relTo.GetOffsetTo(close.ElementAt(1) + native.RelTo, d),
            //     Colors.Red, // group.Color, 
            //     3f);
        }
        
        foreach (var (unit, dest) in assgns)
        {
            var pos = unit.Position;
            var moveType = unit.Template.Entity(d).MoveType.Model(d);
            var path = d.Context.PathCache
                .GetOrAdd((moveType, alliance, pos.GetCell(d),
                    dest.GetNative(d)));
            if (path != null)
            {
                mb.DrawCellPath(relTo, path, group.Color,
                    1f, d);
            }
        }
    }

    public override void RegisterCombatActions(
        UnitGroup g, 
        CombatCalculator combat, LogicWriteKey key)
    {
        // if (Attack == false) return;
        var d = key.Data;
        var units = g.Units.Items(d);
        var natives = Faces.Select(f => f.GetNative(d)).ToHashSet();
        var foreigns = Faces.Select(f => f.GetForeign(d)).ToHashSet();
        foreach (var unit in units)
        {
            var cell = unit.Position.GetCell(d);
            if (natives.Contains(cell) == false) continue;
            var targets = cell.GetNeighbors(d)
                .Intersect(foreigns);
            if (targets.Count() == 0) continue;
            var target = targets.GetRandomElement();
            var attackEdge = UnitAttackEdge.ConstuctAndAddToGraph(target, unit, combat, d);
        }
    }

    public override string GetDescription(Data d)
    {
        return $"Deploying on line from {Faces.First().GetNative(d).Id}" +
               $" to {Faces.Last().GetNative(d).Id}";
    }
}