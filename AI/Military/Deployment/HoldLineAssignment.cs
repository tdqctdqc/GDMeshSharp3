
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineAssignment : GroupAssignment
{
    public Dictionary<int, List<FrontFace<PolyCell>>> FacesByGroupId { get; private set; }
    public static HoldLineAssignment Construct(
        DeploymentAi ai,
        int segId,
        ERef<Regime> regime, 
        LogicWriteKey key)
    {
        var id = ai.DeploymentTreeIds.TakeId(key.Data);
        var a = new HoldLineAssignment(
            id, segId, regime, UnitGroupManager.Construct(regime, id),
            new Dictionary<int, List<FrontFace<PolyCell>>>());
        return a;
    }
    [SerializationConstructor] private HoldLineAssignment(
        int id, int parentId, ERef<Regime> regime, UnitGroupManager groups,
        Dictionary<int, List<FrontFace<PolyCell>>> facesByGroupId) 
        : base(parentId, id, regime, groups)
    {
        FacesByGroupId = facesByGroupId;
    }

    public List<UnitGroup> GetGroupsInOrder(FrontSegment seg, Data d)
    {
        var list = FacesByGroupId
            .Select(v => d.Get<UnitGroup>(v.Key))
            .ToList();
        list.Sort((g, f) =>
        {
            var boundsG = FacesByGroupId[g.Id];
            var gFirst = seg.Frontline.Faces.IndexOf(boundsG.First());
            var gLast = seg.Frontline.Faces.IndexOf(boundsG.Last());
            var boundsF = FacesByGroupId[f.Id];
            var fFirst = seg.Frontline.Faces.IndexOf(boundsF.First());
            var fLast = seg.Frontline.Faces.IndexOf(boundsF.Last());
            if (gFirst < fFirst) return -1;
            if (fFirst < gFirst) return 1;
            if (gLast < fLast) return -1;
            if (fLast < gLast) return 1;
            return 0;
        });
        return list;
    }

    protected override void RemoveGroupFromData(DeploymentAi ai, UnitGroup g)
    {
        FacesByGroupId.Remove(g.Id);
    }

    protected override void AddGroupToData(DeploymentAi ai,
        UnitGroup g, Data d)
    {
        var cell = g.GetCell(d);
        var seg = (FrontSegment)Parent(ai, d);
        if (seg.Frontline.Faces.Any(f => f.Native == cell.Id) == false)
        {
            seg.Insert.AddGroup(ai, g, d);
            return;
        }
        var face = seg.Frontline.Faces
            .First(f => f.Native == cell.Id);
        FacesByGroupId.Add(g.Id, new List<FrontFace<PolyCell>>{face});
    }

    public override float GetPowerPointNeed(Data d)
    {
        var ai = d.HostLogicData.RegimeAis[Regime.Entity(d)]
            .Military.Deployment;
        var seg = (FrontSegment)Parent(ai, d);
        return seg.GetPowerPointNeed(d);
    }

    public override void AdjustWithin(DeploymentAi ai, LogicWriteKey key)
    {
        var d = key.Data;
        var seg = (FrontSegment)Parent(ai, key.Data);
        var lineGroups = GetGroupsInOrder(seg, d);
        if (lineGroups.Count() == 0) return;
        var alliance = seg.Regime.Entity(d).GetAlliance(d);
        var faceCosts = GetFaceCosts(seg, d);
        
        var assgns =
            Assigner.PickInOrderAndAssignAlongFaces<UnitGroup, FrontFace<PolyCell>>(
                seg.Frontline.Faces,
                lineGroups.ToList(),
                g => g.GetPowerPoints(d),
                f => faceCosts[f]
            );
        foreach (var (unitGroup, faces) in assgns)
        {
            var first = faces.X;
            var last = faces.Y;
            if (first > last) throw new Exception();
            FacesByGroupId[unitGroup.Id] = seg.Frontline.Faces.GetRange(first, last - first + 1);
        }
    }


    public override bool PullGroup(DeploymentAi ai, GroupAssignment transferTo,
        LogicWriteKey key)
    {
        var seg = (FrontSegment)Parent(ai, key.Data);
        var lineGroups = FacesByGroupId
            .Select(kvp => key.Data.Get<UnitGroup>(kvp.Key));
        var numUnits = lineGroups.Sum(g => g.Units.Count());
        var smallest = lineGroups.MinBy(g => g.Units.Count());
        UnitGroup de = null;
        if (numUnits - smallest.Units.Count() >= seg.Frontline.Faces.Count)
        {
            de = smallest;
        }

        if (de != null)
        {
            Groups.Transfer(ai, de, transferTo, key);
            return true;
        }

        return false;
    }
    public override PolyCell GetCharacteristicCell(Data d)
    {
        return FacesByGroupId.First().Value.First().GetNative(d);
    }

    public override void GiveOrders(DeploymentAi ai, LogicWriteKey key)
    {
        foreach (var kvp in FacesByGroupId)
        {
            var group = key.Data.Get<UnitGroup>(kvp.Key);
            var bounds = kvp.Value;
            var order = new DeployOnLineGroupOrder(bounds.ToList(), false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }

    private Dictionary<FrontFace<PolyCell>, float> GetFaceCosts(FrontSegment seg, 
        Data d)
    {
        if (seg.Frontline.Faces.Count == 0) return new Dictionary<FrontFace<PolyCell>, float>();
        var alliance = seg.Regime.Entity(d).GetAlliance(d);
        var totalEnemyCost = seg.Frontline
            .Faces.Sum(f => GetFaceEnemyCost(alliance, f, d));
        var totalLengthCost = seg.Frontline.Faces.Count;
        var enemyCostWeight = Front.CoverOpposingWeight;
        var lengthCostWeight = Front.CoverLengthWeight;
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
        if (alliance.IsRivals(foreignAlliance, d) == false)
        {
            throw new Exception();
        }
        float mult = 1f;
        if (alliance.IsAtWar(foreignAlliance, d)) mult = 2f;
        return units.Sum(u => u.GetPowerPoints(d)) * mult;
    }

    public void DissolveInto(DeploymentAi ai, IEnumerable<FrontSegment> segs, LogicWriteKey key)
    {
        foreach (var kvp in FacesByGroupId.ToArray())
        {
            var groupId = kvp.Key;
            var group = key.Data.Get<UnitGroup>(groupId);
            var groupFaces = kvp.Value;
            var seg = segs
                .FirstOrDefault(s => s.Frontline.Faces.Intersect(groupFaces).Count() > 0);
            if (seg == null)
            {
                continue;
            }
            if (seg.HoldLine == this) throw new Exception();
            var newFaces = new List<List<FrontFace<PolyCell>>>();
            seg.Frontline.Faces.DoForRuns(
                groupFaces.Contains,
                r => newFaces.Add(r));
            if (newFaces.Count == 0)
            {
                continue;
            }

            if (seg.HoldLine == this)
            {
                throw new Exception();
            }
            Groups.Transfer(ai, group, seg.HoldLine, key);
        }
        FacesByGroupId.Clear();
    }
}