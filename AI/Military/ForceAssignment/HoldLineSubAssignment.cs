
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
        var assgns =
            Assigner.PickInOrderAndAssignAlongFaces<UnitGroup, FrontFace<PolyCell>>(
                seg.FrontLineFaces,
                lineGroups.ToList(),
                g => g.GetPowerPoints(d),
                f =>
                {
                    var foreignCell = PlanetDomainExt.GetPolyCell(f.Foreign, d);
                    if (foreignCell.Controller.RefId == -1) return 0f;
                    var foreignRegime = foreignCell.Controller.Entity(d);
                    var foreignAlliance = foreignRegime.GetAlliance(d);

                    var units = foreignCell.GetUnits(d);
                    if (units == null || units.Count == 0) return FrontAssignment.PowerPointsPerCellFaceToCover;

                    if (alliance.Rivals.Contains(foreignAlliance) == false) return 0f;
                    float mult = 1f;
                    if (alliance.AtWar.Contains(foreignAlliance)) mult = 2f;
                    return units.Sum(u => u.GetPowerPoints(d)) * mult;
                }
            );
        foreach (var (unitGroup, faces) in assgns)
        {
            var first = seg.FrontLineFaces.IndexOf(faces.First());
            var last = seg.FrontLineFaces.IndexOf(faces.Last());
            if (first > last) throw new Exception();
            BoundsByGroupId[unitGroup.Id] = new Vector2I(first, last);
        }
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