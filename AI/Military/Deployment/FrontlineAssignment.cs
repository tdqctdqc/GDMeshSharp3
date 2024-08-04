
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using MessagePack;

public class FrontlineAssignment : ArmyAssignment
{
    public ERef<Frontline> Frontline { get; private set; }
    public Color Color { get; private set; }
    public HashSet<ERef<Army>> LineGroups { get; private set; }
    public HashSet<ERef<Army>> InsertingGroups { get; private set; }
    
    public static FrontlineAssignment Construct(
        DeploymentAi ai,
        DeploymentBranch parent,
        Frontline frontline,
        LogicKey key)
    {
        return new FrontlineAssignment(ai.IdDispenser.TakeId(),
            parent, ai.Alliance,
            new HashSet<ERef<Army>>(),
            frontline.MakeRef(), ColorsExt.GetRandomColor(),
            new HashSet<ERef<Army>>(), new HashSet<ERef<Army>>());
    }

    public FrontlineAssignment(int id, DeploymentBranch parent, 
        ERef<Alliance> alliance, HashSet<ERef<Army>> armies, 
        ERef<Frontline> frontline, Color color, 
        HashSet<ERef<Army>> lineGroups, 
        HashSet<ERef<Army>> insertingGroups) : base(id, parent, alliance, armies)
    {
        Frontline = frontline;
        Color = color;
        LineGroups = lineGroups;
        InsertingGroups = insertingGroups;
    }


    protected override void RemoveGroupFromData(DeploymentAi ai, Army g)
    {
        LineGroups.Remove(g.MakeRef());
        InsertingGroups.Remove(g.MakeRef());
    }

    public override void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        Frontline.Get(d).Draw(mb, relTo, d);
    }

    protected override void AddGroupToData(DeploymentAi ai,
        Army g, Data d)
    {
        var cell = g.GetHomeCell(d);
        
        if (Frontline.Get(d).Faces.Any(f => f.Native == cell.Id)
            == false)
        {
            InsertingGroups.Add(g.MakeRef());
            return;
        }
        LineGroups.Add(g.MakeRef());
    }

    public override float GetPowerPointNeed(Data d)
    {
        var ai = Alliance.Get(d).GetAi(d).Military.Strategic.FrontlineAis[Frontline];
        
        return ai.AttackWeight + ai.DefendWeight;
    }
    public override Army PullGroup(DeploymentAi ai, 
        Func<Army, float> suitability, 
        LogicKey key)
    {
        if (Armies.Count < 2) return null;
        if (Armies.Sum(g => g.Get(key.Data).Units.Count()) < Frontline.Get(key.Data).Faces.Count * .75f)
        {
            return null;
        }
        if (InsertingGroups.Count > 0)
        {
            var group = InsertingGroups.MaxBy(r => suitability(r.Get(key.Data)));
            Armies.Remove(group);
            InsertingGroups.Remove(group);
            return group.Get(key.Data);
        }
        else if (LineGroups.Count > 0)
        {
            var group = LineGroups.MaxBy(g => suitability(g.Get(key.Data)));
            Armies.Remove(group);
            LineGroups.Remove(group);
            return group.Get(key.Data);
        }

        return null;
    }

    public override float Suitability(Army g, Data d)
    {
        return g.GetPowerPoints(d) + g.Units.Entities(d).Sum(u => u.GetHitPoints(d));
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Frontline.Get(d).Faces.First().GetNative(d);
    }

    public override void GiveOrders(DeploymentAi ai, 
        LogicKey key)
    {
        SetLineAndInsertingGroups(key);
        HandleInsertingGroupsOrders(key);
        if (LineGroups.Count == 0) return;
        var frontline = Frontline.Get(key.Data);
        var frontlineAi = Alliance.Get(key.Data).GetAi(key.Data)
            .Military.Strategic.FrontlineAis[Frontline];
        var lineGroups = LineGroups
            .Select(g => g.Get(key.Data)).ToArray();
        var groupsInOrder = MilUtil.GetLineGroupsInOrder(
            frontline.Faces,
            lineGroups, key.Data);
        
        var lineAssignments = MilUtil
            .GetGroupLineAssignments(Alliance.Get(key.Data),
                LineGroups.Select(g => g.Get(key.Data)), 
                frontline.Faces,
                v => GetFaceCost(frontlineAi, v, key.Data),
                key.Data);
        
        foreach (var (group, lineAssignment)
                 in lineAssignments)
        {
            var order = new LineMission(
                new RefSet<CellRef>(
                    lineAssignment.Select(f => f.MakeRef()).ToHashSet()),
                new RefSet<CellRef>(new HashSet<CellRef>()),
                false);
            var proc = new SetUnitOrderProcedure(
                group.MakeRef(), order);
            key.SendMessage(proc);
        }

        RefSet<CellRef> getAdvanceInto(HashSet<Cell> lineAssignment)
        {
            var res = new HashSet<CellRef>();
            if (frontline.AdvanceInto is null
                || frontline.AdvanceInto.Count == 0)
            {
                return new RefSet<CellRef>(res);
            }
            foreach (var cell in lineAssignment)
            {
                foreach (var n1 in cell.GetNeighbors(key.Data)
                             .Where(c => frontline.AdvanceInto.Contains(c.MakeRef())))
                {
                    res.Add(n1.MakeRef());
                    foreach (var n2 in n1.GetNeighbors(key.Data)
                                 .Where(c => frontline.AdvanceInto.Contains(c.MakeRef())))
                    {
                        res.Add(n2.MakeRef());
                    }
                }
            }

            return  new RefSet<CellRef>(res);
        }
    }

    private float GetFaceCost(FrontlineAi ai, FrontFace f, Data d)
    {
        var atkWeight = ai.FaceAttackWeights.TryGetValue(f, out var w)
            ? w
            : 0f;
        return atkWeight + ai.FaceDefendWeights[f];
    }


    private void SetLineAndInsertingGroups(LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        InsertingGroups = Armies.Where(g =>
        {
            var a = g.Get(key.Data);
            return a.Units.Entities(key.Data)
                .Any(u => frontline.Faces.Any(f => a.Cells.Contains(f.Native))) == false;
        }).ToHashSet();
        LineGroups = Armies.Except(InsertingGroups).ToHashSet();
    }

    private void HandleInsertingGroupsOrders(LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        var ai = Alliance.Get(key.Data).GetAi(key.Data).Military.Strategic.FrontlineAis[Frontline];

        var idealAssignments = MilUtil
            .GetGroupLineAssignments(Alliance.Get(key.Data), 
                Armies.Select(g => g.Get(key.Data)), 
                frontline.Faces,
                v => GetFaceCost(ai, v, key.Data),
                key.Data);
        
        
        foreach (var army in InsertingGroups)
        {
            var close = GetInsertPoint(frontline, army.Get(key.Data),
                key.Data);
            var assignment = idealAssignments[army.Get(key.Data)];
            var order = new LineMission(
                new RefSet<CellRef>(assignment.Select(c => c.MakeRef()).ToHashSet()), 
                new RefSet<CellRef>(new HashSet<CellRef>()),
                false);
            key.SendMessage(new SetUnitOrderProcedure(army, order));
        }
    }

    private Cell GetInsertPoint(Frontline frontline, Army army, Data d)
    {
        return frontline.Faces.Select(f => f.GetNative(d))
            .MinBy(c => c.GetCenter().Offset(army.GetHomeCell(d).GetCenter(), d).Length());
    }
}