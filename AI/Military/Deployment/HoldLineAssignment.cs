
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineAssignment : GroupAssignment
{
    public static float CoverOpposingWeight {get; private set;} = .5f;
    public static float CoverLengthWeight {get; private set;} = 1f;
    public static float DesiredOpposingPpRatio {get; private set;} = 2f;
    public static float PowerPointsPerCellFaceToCover {get; private set;} = 100f;
    public static int IdealSegmentLength = 5;
    public Frontline Frontline { get; private set; }
    public Color Color { get; private set; }
    public HashSet<UnitGroup> LineGroups { get; private set; }
    public HashSet<UnitGroup> InsertingGroups { get; private set; }
    public HoldLineAssignment(
        DeploymentAi ai,
        DeploymentBranch parent,
        Frontline frontline,
        LogicWriteKey key) : base(parent, ai, key)
    {
        Frontline = frontline;
        LineGroups = new HashSet<UnitGroup>();
        InsertingGroups = new HashSet<UnitGroup>();
        Color = ColorsExt.GetRandomColor();
    }
    

    protected override void RemoveGroupFromData(DeploymentAi ai, UnitGroup g)
    {
        LineGroups.Remove(g);
        InsertingGroups.Remove(g);
    }

    protected override void AddGroupToData(DeploymentAi ai,
        UnitGroup g, Data d)
    {
        var cell = g.GetCell(d);
        
        if (Frontline.Faces.Any(f => f.Native == cell.Id)
            == false)
        {
            InsertingGroups.Add(g);
            return;
        }
        LineGroups.Add(g);
    }

    public override float GetPowerPointNeed(Data d)
    {
        var ai = d.HostLogicData.AllianceAis[Alliance]
            .Military.Deployment;
        
        var opposing = GetOpposingPowerPoints(d);
        var length = GetLength(d);

        var oppNeed = opposing * DesiredOpposingPpRatio;
        var lengthNeed = length * PowerPointsPerCellFaceToCover;

        return Mathf.Max(oppNeed, lengthNeed);
    }
    public override UnitGroup PullGroup(DeploymentAi ai, 
        Func<UnitGroup, float> suitability, 
        LogicWriteKey key)
    {
        if (Groups.Count < 2) return null;
        if (Groups.Sum(g => g.Units.Count()) < Frontline.Faces.Count * .75f)
        {
            return null;
        }
        if (InsertingGroups.Count > 0)
        {
            var group = InsertingGroups.MaxBy(suitability);
            Groups.Remove(group);
            InsertingGroups.Remove(group);
            return group;
        }
        else if (LineGroups.Count > 0)
        {
            var group = LineGroups.MaxBy(suitability);
            Groups.Remove(group);
            LineGroups.Remove(group);
            return group;
        }

        return null;
    }

    public override float Suitability(UnitGroup g, Data d)
    {
        return g.GetPowerPoints(d) + g.Units.Items(d).Sum(u => u.GetHitPoints(d));
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Frontline.Faces.First().GetNative(d);
    }

    public override void GiveOrders(DeploymentAi ai, 
        LogicWriteKey key)
    {
        var faceCosts = GetFaceCosts(key.Data);
        var subSegs = GetSubSegs(key, faceCosts);
        var toPick = InsertingGroups.ToHashSet();
        while (toPick.Count > 0)
        {
            var picker = subSegs.MinBy(s => s.Value.have / s.Value.need);
            var values = picker.Value;
            var cell = picker.Key.First().GetNative(key.Data);
            var picked = toPick.MinBy(g =>
                g.GetCell(key.Data).GetCenter().Offset(cell.GetCenter(), key.Data).Length());
            toPick.Remove(picked);
            subSegs[picker.Key] = (values.need, values.have + picked.GetPowerPoints(key.Data));
            var order = GoToCellGroupOrder.Construct(cell, Alliance,
                picked, key.Data);
            key.SendMessage(new SetUnitOrderProcedure(picked.MakeRef(), order));
        }
        
        var inOrder = GetLineGroupsInOrder(key.Data);
        var lineOrders = Assigner.PickInOrderAndAssignAlongFaces(
            Frontline.Faces, inOrder, u => u.GetPowerPoints(key.Data),
            f => faceCosts[f]);
        
        foreach (var (group, bounds) in lineOrders)
        {
            var groupFaces = Frontline.Faces.GetRange(bounds.X, bounds.Y - bounds.X + 1);
            var order = new LineOrder(groupFaces, 
                new List<Cell[]>(), 
                false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }

    private Dictionary<List<FrontFace>, (float need, float have)>
        GetSubSegs(LogicWriteKey key, Dictionary<FrontFace, float> faceCosts)
    {
        var subSegments = new Dictionary<List<FrontFace>, (float need, float have)>();
        for (var i = 0; i < Frontline.Faces.Count; i += 5)
        {
            var from = i;
            var to = Mathf.Min(Frontline.Faces.Count - 1, i + 5);
            var subSeg = Frontline.Faces.GetRange(from, to - from + 1);
            var need = subSeg.Sum(f => faceCosts[f]);
            if (need == 0f) throw new Exception();
            var have = 0f;
            var natives = subSeg.Select(f => f.GetNative(key.Data)).Distinct();
            foreach (var native in natives)
            {
                var units = native.GetUnits((key.Data));
                if (units is null) continue;
                have += units.Sum(u => u.GetPowerPoints(key.Data));
            }

            subSegments.Add(subSeg, (need, have));
        }

        return subSegments;
    }

    public Dictionary<UnitGroup, List<FrontFace>> 
        GetLineAssignments(Data d)
    {

        var inOrder = GetLineGroupsInOrder(d);
        var faceCosts = GetFaceCosts(d);
        var lineOrders = Assigner.PickInOrderAndAssignAlongFaces(
            Frontline.Faces, inOrder, u => u.GetPowerPoints(d),
            f => faceCosts[f]);
        return lineOrders.ToDictionary(kvp => kvp.Key,
            kvp => Frontline.Faces.GetRange(kvp.Value.X, kvp.Value.Y - kvp.Value.X + 1));
    }
    private Dictionary<FrontFace, float> GetFaceCosts(Data d)
    {
        if (Frontline.Faces.Count == 0) return new Dictionary<FrontFace, float>();
        var totalEnemyCost = Frontline
            .Faces.Sum(f => GetFaceEnemyCost(Alliance, f, d));
        var totalLengthCost = Frontline.Faces.Count;
        var enemyCostWeight = CoverOpposingWeight;
        var lengthCostWeight = CoverLengthWeight;
        return Frontline.Faces
            .ToDictionary(f => f,
                f =>
                {
                    float enemyCost;
                    if (totalEnemyCost == 0f)
                    {
                        enemyCost = 0f;
                    }
                    else
                    {
                        enemyCost = enemyCostWeight * GetFaceEnemyCost(Alliance, f, d) / totalEnemyCost;
                    }
                    var lengthCost = lengthCostWeight / totalLengthCost;
                    if (float.IsNaN(lengthCost))
                    {
                        throw new Exception($"length cost weight {lengthCostWeight} total length cost {totalLengthCost}");
                    }
                    var totalCost = enemyCost + lengthCost;
                    if (float.IsNaN(totalCost)) throw new Exception();
                    return totalCost;
                });
    }
    private float GetFaceEnemyCost(Alliance alliance, 
        FrontFace f, Data d)
    {
        var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
        if (foreignCell.Controller.RefId == -1)
        {
            throw new Exception();
        }
        var foreignRegime = foreignCell.Controller.Entity(d);
        var foreignAlliance = foreignRegime.GetAlliance(d);
        var units = foreignCell.GetUnits(d);
        if (units == null || units.Count == 0) return 0f;
        if (alliance.IsRivals(foreignAlliance, d) == false)
        {
            throw new Exception();
        }
        float mult = 1f;
        if (alliance.IsAtWar(foreignAlliance, d)) mult = 2f;
        return units.Sum(u => u.GetPowerPoints(d)) * mult;
    }

    
    public List<UnitGroup> GetLineGroupsInOrder(Data d)
    {
        var list = LineGroups
            .ToList();
        list.Sort((g, f) =>
        {
            var boundsG = g.Units.Items(d)
                .Select(u => u.Position.GetCell(d)).ToHashSet();
            var gFirst = Frontline.Faces
                .FindIndex(f => boundsG.Contains(f.GetNative(d)));
            var gLast = Frontline.Faces
                .FindLastIndex(f => boundsG.Contains(f.GetNative(d)));

            var boundsF = f.Units.Items(d)
                .Select(u => u.Position.GetCell(d));
            var fFirst = Frontline.Faces
                .FindIndex(f => boundsF.Contains(f.GetNative(d)));
            var fLast = Frontline.Faces
                .FindLastIndex(f => boundsF.Contains(f.GetNative(d)));

            if (gFirst == -1 || gLast == -1 || fFirst == -1 || fLast == -1)
            {
                return 0;
            }
            if (gFirst < fFirst) return -1;
            if (fFirst < gFirst) return 1;
            if (gLast < fLast) return -1;
            if (fLast < gLast) return 1;
            return 0;
        });
        return list;
    }
    public float GetOpposingPowerPoints(Data data)
    {
        return Frontline.Faces.Select(f => f.GetNative(data))
            .Distinct()
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(Alliance, data))
            .Sum(n =>
            {
                var us = n.GetUnits(data);
                if (us == null) return 0f;
                return us.Sum(u => u.GetPowerPoints(data));
            });
    }

    public int GetLength(Data d)
    {
        return Frontline.Faces.Count;
    }
}