
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineSubAssignment
{
    public Dictionary<int, (FrontFace<PolyCell>, FrontFace<PolyCell>)> BoundsByGroupId { get; private set; }
    public static HoldLineSubAssignment Construct()
    {
        return new HoldLineSubAssignment(new Dictionary<int, (FrontFace<PolyCell>, FrontFace<PolyCell>)>());
    }
    [SerializationConstructor] private HoldLineSubAssignment(Dictionary<int, (FrontFace<PolyCell>, FrontFace<PolyCell>)> boundsByGroupId) 
    {
        BoundsByGroupId = boundsByGroupId;
    }

    public void Handle(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        AdjustFaceGroups(seg, key.Data);
        GiveLineOrders(seg, key);
    }
    public List<UnitGroup> GetGroupsInOrder(FrontSegmentAssignment seg, Data d)
    {
        var list = BoundsByGroupId
            // .OrderBy(v => seg.FrontLineFaces.IndexOf(v.Value.Item1))
            .Select(v => d.Get<UnitGroup>(v.Key))
            .ToList();
        list.Sort((g, f) =>
        {
            var boundsG = BoundsByGroupId[g.Id];
            var gFirst = seg.FrontLineFaces.IndexOf(boundsG.Item1);
            var gLast = seg.FrontLineFaces.IndexOf(boundsG.Item2);
            var boundsF = BoundsByGroupId[f.Id];
            var fFirst = seg.FrontLineFaces.IndexOf(boundsF.Item1);
            var fLast = seg.FrontLineFaces.IndexOf(boundsF.Item2);
            if (gFirst < fFirst) return -1;
            if (fFirst < gFirst) return 1;
            if (gLast < fLast) return -1;
            if (fLast < gLast) return 1;
            return 0;
        });
        return list;
    }
    private void AdjustFaceGroups(FrontSegmentAssignment seg, 
        Data d)
    {
        var lineGroups = GetGroupsInOrder(seg, d);
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
            BoundsByGroupId[unitGroup.Id] = (seg.FrontLineFaces[first], seg.FrontLineFaces[last]);
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
        if(seg.FrontLineFaces.Contains(face) == false) throw new Exception();
        BoundsByGroupId.Add(g.Id, (face, face));
    }
    private void GiveLineOrders(FrontSegmentAssignment seg,
        LogicWriteKey key)
    {
        foreach (var kvp in BoundsByGroupId)
        {
            var group = key.Data.Get<UnitGroup>(kvp.Key);
            var bounds = kvp.Value;
            var from = seg.FrontLineFaces.IndexOf(bounds.Item1);
            var to = seg.FrontLineFaces.IndexOf(bounds.Item2);
            var line = seg.FrontLineFaces.GetRange(from, to - from + 1);
            var order = new DeployOnLineGroupOrder(line, false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }
}