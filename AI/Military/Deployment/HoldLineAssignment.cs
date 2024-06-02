
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
    public HashSet<Army> LineGroups { get; private set; }
    public HashSet<Army> InsertingGroups { get; private set; }
    public HoldLineAssignment(
        DeploymentAi ai,
        DeploymentBranch parent,
        Frontline frontline,
        LogicWriteKey key) : base(parent, ai, key)
    {
        Frontline = frontline;
        LineGroups = new HashSet<Army>();
        InsertingGroups = new HashSet<Army>();
        Color = ColorsExt.GetRandomColor();
    }
    

    protected override void RemoveGroupFromData(DeploymentAi ai, Army g)
    {
        LineGroups.Remove(g);
        InsertingGroups.Remove(g);
    }

    protected override void AddGroupToData(DeploymentAi ai,
        Army g, Data d)
    {
        var cell = g.GetHomeCell(d);
        
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
    public override Army PullGroup(DeploymentAi ai, 
        Func<Army, float> suitability, 
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

    public override float Suitability(Army g, Data d)
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
        SetLineAndInsertingGroups(key);
        var frontlineFaceCosts 
            = MilAiUtil.GetFaceCosts(Alliance, Frontline.Faces, key.Data);
        HandleInsertingGroupsOrders(key);
        if (LineGroups.Count == 0) return;
        var lineAssignments = MilAiUtil
            .GetGroupLineAssignments(Alliance, LineGroups, Frontline.Faces, key.Data);
        
        var toTake = Frontline.AdvanceInto.ToHashSet();

        
        if (Frontline.AdvanceInto is null
            || Frontline.AdvanceInto.Count == 0)
        {
            foreach (var (group, faces) in lineAssignments)
            {
                var order = new LineMission(faces.Select(f => f.Id).ToHashSet(), 
                    new HashSet<int>(),
                    false);
                var proc = new SetUnitOrderProcedure(
                    group.MakeRef(), order);
                key.SendMessage(proc);
            }

            return;
        }

            
            

        foreach (var (group, faces) in lineAssignments)
        {
            var order = new LineMission(faces.Select(c => c.Id).ToHashSet(),
                new HashSet<int>(), false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }
    }


    private void SetLineAndInsertingGroups(LogicWriteKey key)
    {
        InsertingGroups = Groups.Where(g =>
        {
            return g.Units.Items(key.Data)
                .Any(u => Frontline.Faces.Any(f => g.Cells.Contains(f.Native))) == false;
        }).ToHashSet();
        LineGroups = Groups.Except(InsertingGroups).ToHashSet();
    }

    private void HandleInsertingGroupsOrders(LogicWriteKey key)
    {
        foreach (var army in InsertingGroups)
        {
            var close = GetInsertPoint(army, key.Data);
            var order = new LineMission(
                close.Id.Yield().ToHashSet(), 
                new HashSet<int>(),
                false);
            key.SendMessage(new SetUnitOrderProcedure(army.MakeRef(), order));
        }
    }

    private Cell GetInsertPoint(Army army, Data d)
    {
        return Frontline.Faces.Select(f => f.GetNative(d))
            .MinBy(c => c.GetCenter().Offset(army.GetHomeCell(d).GetCenter(), d).Length());
    }
    public float GetOpposingPowerPoints(Data data)
    {
        return Frontline.Faces.Select(f => f.GetNative(data))
            .Distinct()
            .SelectMany(c => c.GetNeighbors(data))
            .Distinct()
            .Where(n => n.RivalControlled(Alliance, data))
            .Sum(c => data.Context.PowerPoints[c]);
    }

    public int GetLength(Data d)
    {
        return Frontline.Faces.Count;
    }
}