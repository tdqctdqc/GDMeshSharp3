
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineSubAssignment
{
    public Dictionary<int, List<FrontFace<PolyCell>>> FacesByGroupId { get; private set; }
    public static HoldLineSubAssignment Construct()
    {
        return new HoldLineSubAssignment(
            new Dictionary<int, List<FrontFace<PolyCell>>>());
    }
    [SerializationConstructor] private HoldLineSubAssignment(
        Dictionary<int, List<FrontFace<PolyCell>>> facesByGroupId) 
    {
        FacesByGroupId = facesByGroupId;
    }

    public void Handle(FrontSegmentAssignment seg, LogicWriteKey key)
    {
        AdjustFaceGroups(seg, key.Data);
        GiveLineOrders(seg, key);
    }
    public List<UnitGroup> GetGroupsInOrder(FrontSegmentAssignment seg, Data d)
    {
        var list = FacesByGroupId
            .Select(v => d.Get<UnitGroup>(v.Key))
            .ToList();
        list.Sort((g, f) =>
        {
            var boundsG = FacesByGroupId[g.Id];
            var gFirst = seg.Segment.Faces.IndexOf(boundsG.First());
            var gLast = seg.Segment.Faces.IndexOf(boundsG.Last());
            var boundsF = FacesByGroupId[f.Id];
            var fFirst = seg.Segment.Faces.IndexOf(boundsF.First());
            var fLast = seg.Segment.Faces.IndexOf(boundsF.Last());
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
                seg.Segment.Faces,
                lineGroups.ToList(),
                g => g.GetPowerPoints(d),
                f => faceCosts[f]
            );
        foreach (var (unitGroup, faces) in assgns)
        {
            var first = faces.X;
            var last = faces.Y;
            if (first > last) throw new Exception();
            FacesByGroupId[unitGroup.Id] = seg.Segment.Faces.GetRange(first, last - first + 1);
        }
    }

    private Dictionary<FrontFace<PolyCell>, float> GetFaceCosts(FrontSegmentAssignment seg, 
        Data d)
    {
        var alliance = seg.Regime.Entity(d).GetAlliance(d);
        var totalEnemyCost = seg.Segment
            .Faces.Sum(f => GetFaceEnemyCost(alliance, f, d));
        var totalLengthCost = seg.Segment.Faces.Count;
        var enemyCostWeight = FrontAssignment.CoverOpposingWeight;
        var lengthCostWeight = FrontAssignment.CoverLengthWeight;
        return seg.Segment.Faces
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
        if (alliance.IsRivals(foreignAlliance, d) == false)
        {
            throw new Exception();
        }
        float mult = 1f;
        if (alliance.IsAtWar(foreignAlliance, d)) mult = 2f;
        return units.Sum(u => u.GetPowerPoints(d)) * mult;
    }
    public void AddGroupToLine(FrontSegmentAssignment seg,
        UnitGroup g, FrontFace<PolyCell> face)
    {
        if(seg.Segment.Faces.Contains(face) == false) throw new Exception();
        FacesByGroupId.Add(g.Id, new List<FrontFace<PolyCell>>{face});
    }
    private void GiveLineOrders(FrontSegmentAssignment seg,
        LogicWriteKey key)
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

    public void ValidateGroupFaces(FrontSegmentAssignment seg,
        LogicWriteKey key)
    {
        //make sure contiguous
        //and all in seg faces
        foreach (var kvp in FacesByGroupId.ToList())
        {
            var groupId = kvp.Key;
            var faces = kvp.Value;
            faces.RemoveAll(f => seg.Segment.Faces.Contains(f) == false);
            if (faces.Count() == 0)
            {
                FacesByGroupId.Remove(groupId);
                continue;
            }
            for (var i = 0; i < faces.Count - 1; i++)
            {
                var face = faces[i];
                var nextFace = faces[i + 1];
                if (face.Adjacent(nextFace) == false)
                {
                    throw new Exception();
                }
            }
        }
    }

    public void ValidateGroups(LogicWriteKey key)
    {
        var badIds = FacesByGroupId.Keys.Where(id => key.Data.EntitiesById.ContainsKey(id) == false).ToArray();
        foreach (var badId in badIds)
        {
            FacesByGroupId.Remove(badId);
        }
    }
    public void DistributeAmong(IEnumerable<FrontSegmentAssignment> segs, LogicWriteKey key)
    {
        foreach (var kvp in FacesByGroupId)
        {
            var groupId = kvp.Key;
            var groupFaces = kvp.Value;
            var seg = segs
                .FirstOrDefault(s => s.Segment.Faces.Intersect(groupFaces).Count() > 0);
            if (seg == null)
            {
                continue;
            }
            if (seg.HoldLine == this) throw new Exception();
            var newFaces = new List<List<FrontFace<PolyCell>>>();
            seg.Segment.Faces.DoForRuns(
                groupFaces.Contains,
                r => newFaces.Add(r));
            if (newFaces.Count == 0)
            {
                continue;
            }
            
            seg.GroupIds.Add(groupId);
            seg.HoldLine.FacesByGroupId.Add(groupId, newFaces.MaxBy(l => l.Count()));
        }
        FacesByGroupId.Clear();
    }
}