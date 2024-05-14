
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class HoldLineAssignment : GroupAssignment
{
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

        var oppNeed = opposing * MilAiUtil.DesiredOpposingPpRatio;
        var lengthNeed = length * MilAiUtil.PowerPointsPerCellFaceToCover;

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
        var frontlineFaceCosts 
            = MilAiUtil.GetFaceCosts(Alliance, Frontline.Faces, key.Data);
        
        HandleInsertingGroupsOrders(key, frontlineFaceCosts);

        var lineAssignments = MilAiUtil
            .GetLineAssignments(Alliance, Groups, Frontline.Faces, key.Data);
        
        if (Frontline.AdvanceFront is null
            || Frontline.AdvanceFront.Count == 0)
        {
            foreach (var (group, faces) in lineAssignments)
            {
                var order = new LineOrder(faces, 
                    new HashSet<LandCell>(),
                    false);
                var proc = new SetUnitOrderProcedure(
                    group.MakeRef(),
                    order);
                key.SendMessage(proc);
            }

            return;
        }

        var toTake = Frontline.AdvanceInto.ToHashSet();
        var claims = lineAssignments
            .ToDictionary(kvp => kvp.Key,
                kvp => Frontline.AdvanceInto
                .Where(c => kvp.Value.Any(f => f.Foreign == c.Id))
                .OfType<LandCell>()
                .ToHashSet());
        var iter = 0;
        
        toTake.ExceptWith(claims.Values.SelectMany(v => v));
        while (toTake.Count > 0)
        {
            iter++;
            if (iter > 1000)
            {
                throw new Exception("over max iter");
            }
            var frontier = toTake
                .Where(c => c.GetNeighbors(key.Data)
                    .Any(n => Frontline.AdvanceInto.Contains(n)
                              && toTake.Contains(n) == false))
                .OfType<LandCell>()
                .ToArray();
            foreach (var (group, claim) in claims)
            {
                int maxNeighbors = 0;

                for (var i = 0; i < frontier.Length; i++)
                {
                    var fCell = frontier[i];
                    int neighbors = 0;
                    foreach (var claimed in claim)
                    {
                        if (claimed.Neighbors.Contains(fCell.Id))
                        {
                            neighbors++;
                            maxNeighbors = Mathf.Max(neighbors, maxNeighbors);
                            if (neighbors > 1)
                            {
                                claim.Add(fCell);
                                toTake.Remove(fCell);
                                break;
                            }
                        }
                    }
                }

                if (maxNeighbors == 1)
                {
                    var adjs = frontier
                        .Where(f => claim.Any(c => c.Neighbors.Contains(f.Id)));
                    foreach (var adj in adjs)
                    {
                        claim.Add(adj);
                        toTake.Remove(adj);
                    }
                }
            }
        }
        
        foreach (var (group, faces) in lineAssignments)
        {
            var order = new LineOrder(faces,
                claims[group], false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }

    private void HandleInsertingGroupsOrders(LogicWriteKey key,
        Dictionary<FrontFace, float> frontlineFaceCosts)
    {
        var subSegs = GetSubSegs(key, frontlineFaceCosts);
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