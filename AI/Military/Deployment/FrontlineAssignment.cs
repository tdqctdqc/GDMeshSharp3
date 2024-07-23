
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class FrontlineAssignment : GroupAssignment
{
    public Frontline Frontline { get; private set; }
    public Color Color { get; private set; }
    public HashSet<Army> LineGroups { get; private set; }
    public HashSet<Army> InsertingGroups { get; private set; }
    public FrontlineAssignment(
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
        return Frontline.AttackWeight + Frontline.DefendWeight;
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
        return g.GetPowerPoints(d) + g.Units.Entities(d).Sum(u => u.GetHitPoints(d));
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Frontline.Faces.First().GetNative(d);
    }

    public override void GiveOrders(DeploymentAi ai, 
        LogicWriteKey key)
    {
        SetLineAndInsertingGroups(key);
        HandleInsertingGroupsOrders(key);
        if (LineGroups.Count == 0) return;
        
        var lineAssignments = MilUtil
            .GetGroupLineAssignments(Alliance, LineGroups, 
                Frontline.Faces,
                GetFaceCost,
                key.Data);
        
        var toTake = Frontline.AdvanceInto.ToHashSet();

        
        if (Frontline.AdvanceInto is null
            || Frontline.AdvanceInto.Count == 0)
        {
            foreach (var (group, lineAssignment) in lineAssignments)
            {
                var order = new LineMission(
                    new RefSet<CellRef>(lineAssignment.Select(f => f.MakeRef()).ToHashSet()),
                    new RefSet<CellRef>(new HashSet<CellRef>()),
                    false);
                var proc = new SetUnitOrderProcedure(
                    group.MakeRef(), order);
                key.SendMessage(proc);
            }

            return;
        }

            
            

        foreach (var (group, lineAssignment) 
                 in lineAssignments)
        {
            var order = new LineMission(
                
                new RefSet<CellRef>(
                    lineAssignment
                        .Select(c => c.MakeRef()).ToHashSet()),
                getAdvanceInto(lineAssignment),
                true);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(),
                order);
            key.SendMessage(proc);
        }

        RefSet<CellRef> getAdvanceInto(HashSet<Cell> lineAssignment)
        {
            var res = new HashSet<CellRef>();
            foreach (var cell in lineAssignment)
            {
                foreach (var n1 in cell.GetNeighbors(key.Data)
                             .Where(c => Frontline.AdvanceInto.Contains(c)))
                {
                    res.Add(n1.MakeRef());
                    foreach (var n2 in n1.GetNeighbors(key.Data)
                                 .Where(c => Frontline.AdvanceInto.Contains(c)))
                    {
                        res.Add(n2.MakeRef());
                    }
                }
            }

            return  new RefSet<CellRef>(res);
        }
    }

    private float GetFaceCost(FrontFace f)
    {
        var atkWeight = Frontline.FaceAttackWeights.TryGetValue(f, out var w)
            ? w
            : 0f;
        return atkWeight + Frontline.FaceDefendWeights[f];
    }


    private void SetLineAndInsertingGroups(LogicWriteKey key)
    {
        InsertingGroups = Groups.Where(g =>
        {
            return g.Units.Entities(key.Data)
                .Any(u => Frontline.Faces.Any(f => g.Cells.Contains(f.Native))) == false;
        }).ToHashSet();
        LineGroups = Groups.Except(InsertingGroups).ToHashSet();
    }

    private void HandleInsertingGroupsOrders(LogicWriteKey key)
    {
        var idealAssignments = MilUtil
            .GetGroupLineAssignments(Alliance, Groups, 
                Frontline.Faces,
                GetFaceCost,
                key.Data);
        
        
        foreach (var army in InsertingGroups)
        {
            var close = GetInsertPoint(army, key.Data);
            var assignment = idealAssignments[army];
            var order = new LineMission(
                new RefSet<CellRef>(assignment.Select(c => c.MakeRef()).ToHashSet()), 
                new RefSet<CellRef>(new HashSet<CellRef>()),
                false);
            key.SendMessage(new SetUnitOrderProcedure(army.MakeRef(), order));
        }
    }

    private Cell GetInsertPoint(Army army, Data d)
    {
        return Frontline.Faces.Select(f => f.GetNative(d))
            .MinBy(c => c.GetCenter().Offset(army.GetHomeCell(d).GetCenter(), d).Length());
    }
}