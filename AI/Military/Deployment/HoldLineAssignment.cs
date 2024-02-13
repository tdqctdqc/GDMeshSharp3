
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineAssignment : GroupAssignment
{
    public HashSet<int> LineGroups { get; private set; }
    public HashSet<int> InsertingGroups { get; private set; }
    public HoldLineAssignment(
        DeploymentAi ai,
        FrontSegment parent,
        LogicWriteKey key) : base(parent, ai, key)
    {
        LineGroups = new HashSet<int>();
        InsertingGroups = new HashSet<int>();
    }
    

    protected override void RemoveGroupFromData(DeploymentAi ai, UnitGroup g)
    {
        LineGroups.Remove(g.Id);
        InsertingGroups.Remove(g.Id);
    }

    protected override void AddGroupToData(DeploymentAi ai,
        UnitGroup g, Data d)
    {
        var cell = g.GetCell(d);
        var seg = (FrontSegment)Parent;
        
        if (seg.Frontline.Faces.Any(f => f.Native == cell.Id)
            == false)
        {
            InsertingGroups.Add(g.Id);
            return;
        }
        LineGroups.Add(g.Id);
    }

    public override float GetPowerPointNeed(Data d)
    {
        var ai = d.HostLogicData.RegimeAis[Regime.Entity(d)]
            .Military.Deployment;
        var seg = (FrontSegment)Parent;
        return seg.GetPowerPointNeed(d);
    }
    public override UnitGroup PullGroup(DeploymentAi ai, 
        LogicWriteKey key)
    {
        if (InsertingGroups.Count > 0)
        {
            var gId = InsertingGroups.First();
            var group = key.Data.Get<UnitGroup>(gId);
            Groups.Remove(group.MakeRef());
            InsertingGroups.Remove(gId);
            return group;
        }
        else if (LineGroups.Count > 0)
        {
            var gId = LineGroups.First();
            var group = key.Data.Get<UnitGroup>(gId);
            Groups.Remove(group.MakeRef());
            LineGroups.Remove(gId);
            return group;
        }

        return null;
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        return ((FrontSegment)Parent).Frontline.Faces.First().GetNative(d);
    }

    public override void GiveOrders(DeploymentAi ai, 
        LogicWriteKey key)
    {
        var seg = (FrontSegment)Parent;

        foreach (var gId in InsertingGroups)
        {
            var group = key.Data.Get<UnitGroup>(gId);
            var cell = group.GetCell(key.Data);
            var closest = seg.Frontline.Faces
                .MinBy(f => f.GetNative(key.Data).GetCenter().GetOffsetTo(cell.GetCenter(), key.Data).Length());
            var order = GoToCellGroupOrder.Construct(closest.GetNative(key.Data), Regime.Entity(key.Data),
                group, key.Data);
            key.SendMessage(new SetUnitOrderProcedure(group.MakeRef(), order));
        }

        var inOrder = GetGroupsInOrder(seg, key.Data);
        var faceCosts = GetFaceCosts(seg, key.Data);
        var lineOrders = Assigner.PickInOrderAndAssignAlongFaces(
            seg.Frontline.Faces, inOrder, u => u.GetPowerPoints(key.Data),
            f => faceCosts[f]);
        
        foreach (var (group, bounds) in lineOrders)
        {
            var groupFaces = seg.Frontline.Faces.GetRange(bounds.X, bounds.Y - bounds.X + 1);
            var order = new DeployOnLineGroupOrder(groupFaces, false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }

    public Dictionary<UnitGroup, List<FrontFace>> 
        GetLineAssignments(Data d)
    {
        var seg = (FrontSegment)Parent;

        var inOrder = GetGroupsInOrder(seg, d);
        var faceCosts = GetFaceCosts(seg, d);
        var lineOrders = Assigner.PickInOrderAndAssignAlongFaces(
            seg.Frontline.Faces, inOrder, u => u.GetPowerPoints(d),
            f => faceCosts[f]);
        return lineOrders.ToDictionary(kvp => kvp.Key,
            kvp => seg.Frontline.Faces.GetRange(kvp.Value.X, kvp.Value.Y - kvp.Value.X + 1));
    }
    private Dictionary<FrontFace, float> GetFaceCosts(FrontSegment seg, 
        Data d)
    {
        if (seg.Frontline.Faces.Count == 0) return new Dictionary<FrontFace, float>();
        var alliance = seg.Regime.Entity(d).GetAlliance(d);
        var totalEnemyCost = seg.Frontline
            .Faces.Sum(f => GetFaceEnemyCost(alliance, f, d));
        var totalLengthCost = seg.Frontline.Faces.Count;
        var enemyCostWeight = FrontSegment.CoverOpposingWeight;
        var lengthCostWeight = FrontSegment.CoverLengthWeight;
        return seg.Frontline.Faces
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
                        enemyCost = enemyCostWeight * GetFaceEnemyCost(alliance, f, d) / totalEnemyCost;
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

    
    public List<UnitGroup> GetGroupsInOrder(FrontSegment seg, Data d)
    {
        var list = LineGroups
            .Select(id => d.Get<UnitGroup>(id))
            .ToList();
        list.Sort((g, f) =>
        {
            var boundsG = g.Units.Items(d)
                .Select(u => u.Position.GetCell(d)).ToHashSet();
            var gFirst = seg.Frontline.Faces
                .FindIndex(f => boundsG.Contains(f.GetNative(d)));
            var gLast = seg.Frontline.Faces
                .FindLastIndex(f => boundsG.Contains(f.GetNative(d)));

            var boundsF = f.Units.Items(d)
                .Select(u => u.Position.GetCell(d));
            var fFirst = seg.Frontline.Faces
                .FindIndex(f => boundsF.Contains(f.GetNative(d)));
            var fLast = seg.Frontline.Faces
                .FindLastIndex(f => boundsF.Contains(f.GetNative(d)));

            if (gFirst == -1 || gLast == -1 || fFirst == -1 || fLast == -1)
            {
                throw new Exception();
            }
            if (gFirst < fFirst) return -1;
            if (fFirst < gFirst) return 1;
            if (gLast < fLast) return -1;
            if (fLast < gLast) return 1;
            return 0;
        });
        return list;
    }
}