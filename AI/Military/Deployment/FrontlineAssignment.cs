
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
    public float AttackWeight { get; private set; }
    public float DefendWeight { get; private set; }
    public Dictionary<FrontFace, float> FaceAttackWeights { get; private set; }
    public Dictionary<FrontFace, float> FaceDefendWeights { get; private set; }
    public Dictionary<List<FrontFace>, Army> ArmyFaceAssignments { get; private set; }
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
            new HashSet<ERef<Army>>(), 
            new HashSet<ERef<Army>>(),
            0f, 0f, new Dictionary<FrontFace, float>(),
            new Dictionary<FrontFace, float>(),
            new Dictionary<List<FrontFace>, Army>());
    }

    public FrontlineAssignment(int id, DeploymentBranch parent, 
        ERef<Alliance> alliance, 
        HashSet<ERef<Army>> armies, 
        ERef<Frontline> frontline, Color color, 
        HashSet<ERef<Army>> lineGroups, 
        HashSet<ERef<Army>> insertingGroups, 
        float attackWeight, float defendWeight, 
        Dictionary<FrontFace, float> faceAttackWeights, 
        Dictionary<FrontFace, float> faceDefendWeights,
        Dictionary<List<FrontFace>, Army> armyFaceAssignments) : base(id, parent, alliance, armies)
    {
        Frontline = frontline;
        Color = color;
        AttackWeight = attackWeight;
        DefendWeight = defendWeight;
        FaceAttackWeights = faceAttackWeights;
        FaceDefendWeights = faceDefendWeights;
        ArmyFaceAssignments = armyFaceAssignments;
    }


    protected override void RemoveArmyFromData(Army g)
    {
    }

    public override void Draw(MeshBuilder mb, Vector2 relTo, Data d)
    {
        Frontline.Get(d).Draw(mb, relTo, d);
    }

    protected override void AddGroupToData(
        Army g, Data d)
    {
    }

    public override float GetPowerPointNeed(Data d)
    {
        return AttackWeight + DefendWeight;
    }

    public void SetupFrontSegments(LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        var segs = frontline.Faces.GetSegmentsOfApproxLength(
            Army.CommandRadius * 2 - 2);
        foreach (var seg in segs)
        {
            ArmyFaceAssignments.Add(seg, null);
        }
    }
    public HashSet<Army> AssignArmiesToSegs(LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        
        var alliance = Alliance.Get(key.Data);
        if (Armies.Count > 0)
        {
            var segs = ArmyFaceAssignments.Keys.ToList();
            var armyAssignment = OrToolsExt
                .GetAssignment(Armies.Select(a => a.Get(key.Data)).ToList(),
                    segs,
                    (army, list) =>
                    {
                        var mid = list.GetMiddleElement();
                        return (int)army.GetMoveCost(mid.GetNative(key.Data), key.Data);
                    },
                    out var leftoverArmies,
                    out var leftoverSegs, out var costs
                );
            foreach (var (army, value) in armyAssignment)
            {
                var cost = costs[(army, value)];
                if (cost > army.MoveType(key.Data).BaseSpeed * 2f)
                {
                    leftoverArmies.Add(army);
                }
                else
                {
                    ArmyFaceAssignments[value] = army;
                }
            }
            foreach (var army in leftoverArmies)
            {
                RemoveArmy(army);
            }

            return leftoverArmies;
        }
        else
        {
            return new HashSet<Army>();
        }
    }
    
    
    
    
    

    public override void SetWeights(LogicKey key)
    {
        var d = key.Data;
        var alliance = Alliance.Get(d);
        var frontline = Frontline.Get(d);
        var length = frontline.Faces.Count;
        var report = new FrontlineTacticalReport(frontline, d);
        var hostilePp = report.HostileOnFront.Any()
            ? report.HostileOnFront.Sum(
                c => d.Context.PowerPoints[c]) 
            : 0f;
        hostilePp *= .5f;
        var rivalPp = report.RivalOnFront.Sum(
            c => d.Context.PowerPoints[c]) * 5f;
        var opposing = hostilePp + rivalPp;
        var oppNeed = opposing * MilUtil.DesiredOpposingPpRatio;
        var lengthNeed = length * MilUtil.PowerPointsPerCellFaceToCover;
            
        AddDefendWeightAlongWholeLine(oppNeed + lengthNeed, d);
        
        var friendlyPower = report.FriendlyPower;
        var enemyPower = report.EnemyPower;
        var availablePowerForOffense = friendlyPower - enemyPower * .8f;
        
        if (availablePowerForOffense <= 0f) return;
        var allHostile = report.HostileOnFront.ToHashSet();

        foreach (var pocket in report.Pockets.OrderBy(v => v.Count))
        {
            var pocketPower = pocket.Sum(c => d.Context.PowerPoints[c]);
            var commit = 1.5f * pocketPower;
            availablePowerForOffense -= commit;
            AddAttackWeight(commit, pocket, key);
            allHostile.ExceptWith(pocket);
            if (availablePowerForOffense <= 0f) break;
        }
        
        foreach (var hostile in allHostile.OrderBy(getAtkScore))
        {
            if (availablePowerForOffense <= 0f) break;
            var commit = d.Context.PowerPoints[hostile] * 1.5f;
            availablePowerForOffense -= commit;
            AddAttackWeight(commit, hostile, key);
        }
        
        float getAtkScore(Cell hCell)
        {
            var pp = d.Context.PowerPoints[hCell];
            var adj = hCell.GetNeighbors(d)
                .Count(c => c.Controller.RefId == alliance.Id);
            return pp / adj;
        }
        
    }

    // public override Army PullGroup(Func<Army, float> suitability, 
    //     LogicKey key)
    // {
    //     if (Armies.Count < 2) return null;
    //     if (Armies.Sum(g => g.Get(key.Data).Units.Count()) < Frontline.Get(key.Data).Faces.Count * .75f)
    //     {
    //         return null;
    //     }
    //     if (InsertingGroups.Count > 0)
    //     {
    //         var group = InsertingGroups.MaxBy(r => suitability(r.Get(key.Data)));
    //         Armies.Remove(group);
    //         InsertingGroups.Remove(group);
    //         return group.Get(key.Data);
    //     }
    //     else if (LineGroups.Count > 0)
    //     {
    //         var group = LineGroups.MaxBy(g => suitability(g.Get(key.Data)));
    //         Armies.Remove(group);
    //         LineGroups.Remove(group);
    //         return group.Get(key.Data);
    //     }
    //
    //     return null;
    // }

    public float Suitability(Army g, Data d)
    {
        return g.GetPowerPoints(d) + g.Units.Entities(d).Sum(u => u.GetHitPoints(d));
    }

    public override float Suitability(Unit u, Data d)
    {
        return u.GetPowerPoints(d);
    }

    public override Cell GetCharacteristicCell(Data d)
    {
        return Frontline.Get(d).Faces.First().GetNative(d);
    }

    public override Unit PullUnit(Func<Unit, float> suitability, LogicKey key)
    {
        var maxArmy = Armies.Select(a => a.Get(key.Data))
            .MaxBy(a => a.GetPowerPoints(key.Data));
        return maxArmy.Units.Entities(key.Data).MaxBy(suitability);
    }

    public override void GiveOrders(LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        foreach (var (faces, army)
                 in ArmyFaceAssignments)
        {
            var cells = faces
                .Select(f => f.GetNative(key.Data))
                .ToHashSet();
            
            var order = new LineMission(
                new RefSet<CellRef>(
                    cells.Select(c => c.MakeRef()).ToHashSet()),
                getAdvanceInto(cells),
                false);
            var proc = new SetUnitOrderProcedure(
                army.MakeRef(), order);
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

    private float GetFaceCost(FrontFace f, Data d)
    {
        var atkWeight = FaceAttackWeights.TryGetValue(f, out var w)
            ? w
            : 0f;
        return atkWeight + FaceDefendWeights[f];
    }


    

    private Cell GetInsertPoint(Frontline frontline, Army army, Data d)
    {
        return frontline.Faces.Select(f => f.GetNative(d))
            .MinBy(c => c.GetCenter().Offset(army.GetHomeCell(d).GetCenter(), d).Length());
    }
    
    public void AddAttackWeight(float w,
        Cell attack, LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);

        AttackWeight += w;
        for (var i = 0; i < frontline.Faces.Count; i++)
        {
            var face = frontline.Faces[i];
            if (face.Foreign == attack.Id)
            {
                FaceAttackWeights.AddOrSum(face, w);
            }
        }

        var newAdvanceInto = frontline.
            AdvanceInto.Concat(attack.MakeRef().Yield())
                .ToHashSet();
        var proc = new SetFrontlineAdvanceIntoProcedure(Frontline, newAdvanceInto);
        key.SendMessage(proc);
    }
    public void AddAttackWeight(float w,
        IEnumerable<Cell> attack, LogicKey key)
    {
        var frontline = Frontline.Get(key.Data);
        AttackWeight += w;
        var ids = attack.Select(a => a.Id).ToHashSet();
        
        for (var i = 0; i < frontline.Faces.Count; i++)
        {
            var face = frontline.Faces[i];
            if (ids.Contains(face.Foreign))
            {
                FaceAttackWeights.AddOrSum(face, w);
            }
        }
        var newAdvanceInto = frontline.
            AdvanceInto.Union(attack.Select(a => a.MakeRef()))
                .ToHashSet();
        var proc = new SetFrontlineAdvanceIntoProcedure(Frontline, newAdvanceInto);
        key.SendMessage(proc);
    }
    
    public void AddDefendWeightAlongWholeLine(float w, Data d)
    {
        var frontline = Frontline.Get(d);
        var alliance = frontline.Alliance.Get(d);
        DefendWeight += w;
        var totalCellDef = frontline.Faces.Sum(
            f => 1f / ((LandCell)f.GetNative(d)).GetLandDefendScore(d));
        
        for (var i = 0; i < frontline.Faces.Count; i++)
        {
            var face = frontline.Faces[i];
            var native = (LandCell)face.GetNative(d);
            var foreign = face.GetForeign(d);
            var mult = foreign.Controller.Get(d)
                .GetAlliance(d).IsAtWar(alliance, d)
                ? 1f : global::Frontline.DefMultForNotAtWarCell;
            if (totalCellDef == 0f)
            {
                FaceDefendWeights.AddOrSum(face, w / frontline.Faces.Count);
            }
            else
            {
                var cellDefRatio = (1f / native.GetLandDefendScore(d)) / totalCellDef;
                FaceDefendWeights.AddOrSum(face, cellDefRatio * w);
            }
        }
    }
}