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
            ShiftGroupsFromOldFrontlines(key, oldFrontlineGroupAssignments, milAi);
        }

        Root.GrabUnassignedGroups(key);
        Root.SetWeights(key);
        Root.ShiftGroups(key);
        Root.GiveOrders(key);
    }

    private void ShiftGroupsFromOldFrontlines(LogicKey key, 
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


    public DeploymentRoot GetRoot()
    {
        return Root;
    }
}