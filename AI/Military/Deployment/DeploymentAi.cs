using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using MessagePack;

public class DeploymentAi
{
    public ERef<Alliance> Alliance { get; private set; }
    public DeploymentRoot Root { get; private set; }
    public IdDispenser IdDispenser { get; private set; }
    public static DeploymentAi Construct(Alliance a, Data d)
    {
        var ai = new DeploymentAi(a.MakeRef(),
            null,
            new IdDispenser(0));
        return ai;
    }

    public DeploymentAi(ERef<Alliance> alliance, DeploymentRoot root, IdDispenser idDispenser)
    {
        Alliance = alliance;
        Root = root;
        IdDispenser = idDispenser;
    }

    public void Clear(LogicKey key)
    {
        IdDispenser = new IdDispenser(0);
        Root = null;
    }
    public void Calculate(Alliance alliance, LogicKey key)
    {
        var oldFrontlineGroupAssignments = 
            Root
            ?.GetDescendentAssignmentsOfType<FrontlineAssignment>()
            .ToDictionary(v => v.Frontline,
                v => v.Armies.ToHashSet());
        var milAi = alliance.GetAi(key.Data).Military;
        Clear(key);
        Root = new DeploymentRoot(alliance.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            new HashSet<DeploymentBranch>(), 
            new HashSet<ArmyAssignment>());
        
        Root.MakeTheaters(milAi, key);
        if (oldFrontlineGroupAssignments is not null)
        {
            ShiftArmiesFromOldFrontlines(key, oldFrontlineGroupAssignments, milAi);
        }
        CreateArmiesForEmptyFronts(key);

        // Root.GrabUnassignedGroups(key);
        Root.SetWeights(key);
        Root.ShiftGroups(key);
        Root.GiveOrders(key);

    }
    
    private void ShiftArmiesFromOldFrontlines(LogicKey key, 
        Dictionary<ERef<Frontline>, HashSet<ERef<Army>>> oldFrontlineGroupAssignments,
        AllianceMilitaryAi milAi)
    {
        var context = milAi.Strategic.Context;
        var newFrontlineAssignments = Root.GetDescendentAssignmentsOfType<FrontlineAssignment>()
            .ToDictionary(v => v.Frontline, v => v);
        
        foreach (var (oldFrontline, armies) 
                 in oldFrontlineGroupAssignments)
        {
            if (milAi.Strategic
                    .FrontlineMerges.ContainsKey(oldFrontline) == false)
                continue;
            var mergeFrontlines = milAi.Strategic
                .FrontlineMerges[oldFrontline]
                .Select(fl => fl.Get(key.Data))
                .ToArray();
            if (mergeFrontlines.Length == 0)
            {
                continue;
            }
            // GD.Print("found merge frontline");
            foreach (var armyRef in armies)
            {
                if (key.Data.HasEntity(armyRef.RefId) == false) continue;
                var army = armyRef.Get(key.Data);
                var merge = mergeFrontlines
                    .FirstOrDefault(m => 
                        m.Faces.Any(f => army.LineMission.LineCells.Contains(f.Native)));
                
                // GD.Print($"shifted army at {home.Id}");

                if (merge is null 
                    || newFrontlineAssignments.ContainsKey(merge.MakeRef()) == false)
                {
                    GD.Print("couldnt find assignment for frontline");
                    continue;
                }
                var mergeAssignment = newFrontlineAssignments[merge.MakeRef()];
                mergeAssignment.PushGroup(army, key);
            }
        }
    }
    private void CreateArmiesForEmptyFronts(LogicKey key)
    {
        var alliance = Alliance.Get(key.Data);
        var regimes = alliance.Members.Entities(key.Data);

        var frontlineAssgns = Root
            .GetDescendentAssignmentsOfType<FrontlineAssignment>()
            .ToArray();

        foreach (var frontlineAssgn in frontlineAssgns)
        {
            var frontline = frontlineAssgn.Frontline.Get(key.Data);
            var segs = frontline.Faces.GetSegmentsOfApproxLength(
                Army.CommandRadius / 2 + 1);

            if (frontlineAssgn.Armies.Count > 0)
            {
                var armyAssignment = OrToolsExt
                    .GetLinearSumAssignment(frontlineAssgn.Armies.Select(a => a.Get(key.Data)).ToList(),
                        segs,
                        (army, list) =>
                        {
                            var mid = list.GetMiddleElement();
                            var path = army.FindArmyPath(mid.GetNative(key.Data), true, key.Data);
                            return (int)PathFinder<Cell>.GetPathCost(
                                path, (c1, c2) => army.MoveType(key.Data).EdgeCost(c1, c2, key.Data));
                        });
                foreach (var (army, value) in armyAssignment)
                {
                    frontlineAssgn.ArmyFaceAssignments
                        .Add(value, army);
                }
            }

            var uncoveredSegs = segs
                .Where(s => frontlineAssgn.ArmyFaceAssignments
                    .ContainsKey(s) == false)
                .ToArray();
            foreach (var uncoveredSeg in uncoveredSegs)
            {
                var army = Army.Create(alliance.Leader.Get(key.Data),
                    uncoveredSeg.Select(f => f.GetNative(key.Data)),
                    new int[] { },
                    key);
                frontlineAssgn.ArmyFaceAssignments.Add(uncoveredSeg, army);
            }
        }
    }

    public DeploymentRoot GetRoot()
    {
        return Root;
    }
}