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
        var context = new StrategicContext(Alliance.Get(key.Data),
            milAi.Strategic.PrevOccupation.Select(p => p.Get(key.Data)).ToHashSet(),
            key.Data);
        var newFrontlineAssignments = Root.GetDescendentAssignmentsOfType<FrontlineAssignment>()
            .ToDictionary(v => v.Frontline, v => v);
        
        foreach (var (oldFrontline, armies) 
                 in oldFrontlineGroupAssignments)
        {
            var merges = milAi.Strategic.FrontlineMerges[oldFrontline];
            if (merges.Count == 0)
            {
                
                continue;
            }
            // GD.Print("found merge frontline");
            foreach (var armyRef in armies)
            {
                if (key.Data.HasEntity(armyRef.RefId) == false) continue;
                var army = armyRef.Get(key.Data);
                var home = army.GetHomeCell(key.Data);
                var union = context.Unions
                    .FirstOrDefault(u => u.Contains(home));
                if (union is null) continue;
                var merge = merges
                    .FirstOrDefault(m =>
                    {
                        var first = m.Get(key.Data).Faces.First()
                            .GetNative(key.Data);
                        return union.Contains(first);
                    });
                if (merge.IsEmpty())
                {
                    GD.Print($"skipping army at {home.Id}");
                    // var issue = new CustomIssue(pos,
                    //     $"{leader.Name} merge empty",
                    //     c =>
                    //     {
                    //         foreach (var cell in union)
                    //         {
                    //             c.DrawPolygonRel(cell.AbsBoundary(key.Data).ToArray(),
                    //                 Colors.Blue, pos, key.Data);
                    //         }
                    //
                    //         c.DrawFrontFaces(fl.Faces, Colors.Red, 5f, pos, key.Data);
                    //     });
                    // key.Data.ClientPlayerData.Issues.Add(issue);
                    continue;
                }
                // GD.Print($"shifted army at {home.Id}");

                if (newFrontlineAssignments.ContainsKey(merge) == false)
                {
                    GD.Print("couldnt find assignment for frontline");
                    continue;
                }
                var mergeAssignment = newFrontlineAssignments[merge];
                mergeAssignment.PushGroup(army, key);
            }
        }
    }


    public DeploymentRoot GetRoot()
    {
        return Root;
    }
}