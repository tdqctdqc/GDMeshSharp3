
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineSubAssignment
{
    public Dictionary<int, Vector2I> BoundsByGroupId { get; private set; }
    public static HoldLineSubAssignment Construct()
    {
        return new HoldLineSubAssignment(new Dictionary<int, Vector2I>());
    }
    [SerializationConstructor] private HoldLineSubAssignment(Dictionary<int, Vector2I> boundsByGroupId) 
    {
        BoundsByGroupId = boundsByGroupId;
    }

    public void Handle(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        AdjustFaceGroups(seg, key.Data);
        GiveLineOrders(seg, key);
    }
    public List<UnitGroup> GetGroupsInOrder(Data d)
    {
        var list = BoundsByGroupId
            .Select(v => d.Get<UnitGroup>(v.Key))
            .ToList();
        list.Sort((g, f) =>
        {
            var boundsG = BoundsByGroupId[g.Id];
            var boundsF = BoundsByGroupId[f.Id];
            return boundsG.Compare(boundsF);
        });
        return list;
    }
    private void AdjustFaceGroups(FrontSegmentAssignment seg, 
        Data d)
    {
        var lineGroups = GetGroupsInOrder(d);
        if (lineGroups.Count() == 0) return;
        var alliance = seg.Regime.Entity(d).GetAlliance(d);
        var faceCosts = GetFaceCosts(seg, d);
        
        var assgns =
            Assigner.PickInOrderAndAssignAlongFaces<UnitGroup, FrontFace<PolyCell>>(
                seg.FrontLineFaces,
                lineGroups.ToList(),
                g => g.GetPowerPoints(d),
                f => faceCosts[f]
            );
        foreach (var (unitGroup, faces) in assgns)
        {
            var first = faces.X;
            var last = faces.Y;
            if (first > last) throw new Exception();
            BoundsByGroupId[unitGroup.Id] = new Vector2I(first, last);
        }
    }

    private Dictionary<FrontFace<PolyCell>, float> GetFaceCosts(FrontSegmentAssignment seg, 
        Data d)
    {
        var alliance = seg.Regime.Entity(d).GetAlliance(d);
        var totalEnemyCost = seg.FrontLineFaces
            .Sum(f => GetFaceEnemyCost(alliance, f, d));
        var totalLengthCost = seg.FrontLineFaces.Count;
        var enemyCostWeight = FrontAssignment.CoverOpposingWeight;
        var lengthCostWeight = FrontAssignment.CoverLengthWeight;
        return seg.FrontLineFaces
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
                    var totalCost = enemyCost + lengthCost;
                    if (float.IsNaN(totalCost)) throw new Exception();
                    return totalCost;
                });
    }
    private float GetFaceEnemyCost(Alliance alliance, 
        FrontFace<PolyCell> f, Data d)
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
        if (alliance.Rivals.Contains(foreignAlliance) == false)
        {
            throw new Exception();
        }
        float mult = 1f;
        if (alliance.AtWar.Contains(foreignAlliance)) mult = 2f;
        return units.Sum(u => u.GetPowerPoints(d)) * mult;
    }
    public void AddGroupToLine(FrontSegmentAssignment seg,
        UnitGroup g, FrontFace<PolyCell> face)
    {
        var index = seg.FrontLineFaces.IndexOf(face);
        if (index == -1) throw new Exception();
        BoundsByGroupId.Add(g.Id, new Vector2I(index, index));
    }
    private void GiveLineOrders(FrontSegmentAssignment seg,
        LogicWriteKey key)
    {
        foreach (var kvp in BoundsByGroupId)
        {
            var group = key.Data.Get<UnitGroup>(kvp.Key);
            var bounds = kvp.Value;
            var line = seg.FrontLineFaces.GetRange(bounds.X, bounds.Y - bounds.X + 1);
            var order = new DeployOnLineGroupOrder(line, false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }
}