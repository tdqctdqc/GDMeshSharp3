
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LineOrder : UnitGroupOrder
{
    public List<FrontFace> Faces { get; private set; }
    public List<Cell[]> FaceAdvanceRoutes { get; private set; }
    
    public bool Advance { get; private set; }
    public LineOrder(List<FrontFace> faces, 
        List<Cell[]> faceAdvanceRoutes,
        bool advance)
    {
        FaceAdvanceRoutes = faceAdvanceRoutes;
        Faces = faces;
        Advance = advance;
    }

    public override void Handle(UnitGroup g, LogicWriteKey key,
        HandleUnitOrdersProcedure proc)
    {
        var units = g.Units.Items(key.Data);
        var alliance = g.Regime.Get(key.Data).GetAlliance(key.Data);
        var assgn = GetAssignments(g, key.Data);
        foreach (var unit in units)
        {
            var dest = PlanetDomainExt
                .GetPolyCell(assgn[unit].Native, key.Data);
            var pos = unit.Position.Copy();
            var moveType = unit.Template.Get(key.Data).MoveType.Get(key.Data);
            var movePoints = moveType.BaseSpeed;
            var moveCtx = new MoveData(unit.Id, moveType, movePoints, alliance);
            pos.MoveToCell(moveCtx, dest, key);
            proc.NewUnitPosesById.TryAdd(unit.Id, pos);
        }
    }

    private Dictionary<Unit, FrontFace> GetAssignments(UnitGroup g, Data d)
    {
        var units = g.Units.Items(d);
        var alliance = g.Regime.Get(d).GetAlliance(d);
        return Assigner
            .PickBestAndAssignAlongFacesSingle<Unit, FrontFace>
            (
                Faces,
                units,
                u => u.GetPowerPoints(d),
                (u, f) => 
                    u.Position.GetCell(d).GetCenter().Offset(
                        PlanetDomainExt.GetPolyCell(f.Native, d).GetCenter(), d).Length(),
                f =>
                {
                    return 1f;
                    var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
                    if (foreignCell.Controller.RefId == -1) return 0f;
                    var foreignRegime = foreignCell.Controller.Get(d);
                    var foreignAlliance = foreignRegime.GetAlliance(d);
                    var units = foreignCell.GetUnits(d);
                    if (units == null || units.Any() == false) return HoldLineAssignment.PowerPointsPerCellFaceToCover;
                    if (alliance.IsRivals(foreignAlliance, d) == false) return 0f;
                    float mult = HoldLineAssignment.DesiredOpposingPpRatio;
                    return units.Sum(u => u.GetPowerPoints(d)) * mult;
                }
            );
    }
    public override void Draw(UnitGroup group, Vector2 relTo, 
        MeshBuilder mb, Data d)
    {
        var innerColor = group.Regime.Get(d).PrimaryColor;
        var outerColor = group.Regime.Get(d).PrimaryColor;
        var squareSize = 10f;
        var lineSize = 5f;
        var alliance = group.Regime.Get(d).GetAlliance(d);
        var assgns = GetAssignments(group, d);

        var natives = Faces.Select(f => f.GetNative(d)).Distinct();
        var foreigns = Faces.Select(f => f.GetForeign(d)).Distinct();
        foreach (var n in natives)
        {
            mb.DrawPolygon(n.RelBoundary.Select(p => relTo.Offset(p + n.RelTo, d)).ToArray(),
                new Color(Colors.Blue, .5f));
        }
        foreach (var n in foreigns)
        {
            mb.DrawPolygon(n.RelBoundary.Select(p => relTo.Offset(p + n.RelTo, d)).ToArray(),
                new Color(Colors.Red, .5f));
        }
        
        foreach (var (unit, dest) in assgns)
        {
            var pos = unit.Position;
            var moveType = unit.Template.Get(d).MoveType.Get(d);
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
        // if(Advance == false) return;
        if (FaceAdvanceRoutes == null 
            || FaceAdvanceRoutes.Count == 0) return;
        var d = key.Data;
        var units = g.Units.Items(d);
        var assignments = GetAssignments(g, d);
        foreach (var unit in units)
        {
            var face = assignments[unit];
            var target = face.GetForeign(d);
            var index = Faces.IndexOf(face);
            var route = FaceAdvanceRoutes[index];
            if (route == null || route.Length == 0)
            {
                continue;
            }

            if (route[0] != target) throw new Exception();
            UnitAttackEdge.ConstuctAndAddToGraph(target, unit, combat, d);
        }
    }

    public override string GetDescription(Data d)
    {
        return $"Deploying on line from {Faces.First().GetNative(d).Id}" +
               $" to {Faces.Last().GetNative(d).Id}";
    }
}