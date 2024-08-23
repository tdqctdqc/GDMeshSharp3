using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using MessagePack;

public class DeploymentAi
{
    public DeploymentRoot Root { get; private set; }
    public IdDispenser IdDispenser { get; private set; }
    public static DeploymentAi Construct(Regime a, Data d)
    {
        var ai = new DeploymentAi(null, 
            new IdDispenser(0));
        return ai;
    }

    public DeploymentAi(DeploymentRoot root, IdDispenser idDispenser)
    {
        Root = root;
        IdDispenser = idDispenser;
    }

    public void Clear(LogicKey key)
    {
        IdDispenser = new IdDispenser(0);
        Root = null;
    }
    public void Calculate(Regime regime, TimerTreeNode parentTimer, LogicKey key)
    {
        var timer = parentTimer.AddChildAndStart("Deployment");
        
        
        
        var milAi = regime.GetAi(key.Data).Military;
        var stratAi = milAi.Strategic;
        var oldFrontlineGroupAssignments = 
            Root
            ?.GetDescendentAssignmentsOfType<FrontlineAssignment>()
            .ToDictionary(v => v.Frontline,
                v => v.Armies.ToHashSet());
        var oldTheaterAssignments = 
            Root
            ?.GetDescendentNodesOfType<TheaterBranch>()
                .ToArray();
        var oldRoot = Root;
        Root = new DeploymentRoot(regime.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            new HashSet<DeploymentBranch>(), 
            new HashSet<ArmyAssignment>());
        timer.RunChild("Making theaters",
            () => Root.MakeTheaters(milAi, key));
        if (oldRoot is not null)
        {
            timer.RunChild("merging to new",
                () => oldRoot.MergeToNew(Root, stratAi.Context, key));
        }

        PatchFrontlines(regime, timer, key);
        
        timer.RunChild("setting weights",
            () => Root.SetWeights(key));
        
        timer.RunChild("theaters pulling units",
            () => TheatersPullUnits(regime, key));
        
        timer.RunChild("shifting units",
            () => Root.ShiftUnits(key));
        
        timer.RunChild("giving orders",
            () => Root.GiveOrders(key));
        // ClearUnusedArmies(regime, key);
        timer.Stop();
    }
    

    private void PatchFrontlines(Regime regime, 
        TimerTreeNode parentTimer,
        LogicKey key)
    {
        var timer = parentTimer.AddChildAndStart("patching frontlines");
        var theaters = Root
            .GetDescendentNodesOfType<TheaterBranch>()
            .ToArray();
        foreach (var theater in theaters)
        {
            var frontlineAssgns 
                = theater.GetDescendentAssignmentsOfType<FrontlineAssignment>();
            
            timer.RunChild("setting up front segs", () =>
            {
                foreach (var fa in frontlineAssgns)
                {
                    fa.SetupFrontSegments(key);
                }
            });
            
            
            var freeArmies = timer.RunChild("making assignments", () =>
            {
                return frontlineAssgns
                    .SelectMany(fa => fa.AssignArmiesToSegs(key)).ToHashSet();
            });
            if (freeArmies.Count > 0)
            {

                var uncoveredSegsBefore = timer.RunChild("finding uncovered segs before", () =>
                {
                    return frontlineAssgns
                        .SelectMany(fa =>
                            fa.ArmyFaceAssignments.Where(kvp => kvp.Value == null).Select(kvp => (fa, kvp.Key)))
                        .ToDictionary(v => v.Key, v => v.fa);
                });
                
                
                var assignments 
                    = timer.RunChild("making assignments for leftover", () =>
                    {
                        return FrontlineAssignment.MakeArmyAssignments(
                            freeArmies.ToList(),
                            uncoveredSegsBefore.Keys.ToList(),
                            key.Data);
                    });
                var leftoverArmies = freeArmies
                    .Except(assignments.Keys).ToList();
                

                timer.RunChild("finalize and cleanup", () =>
                {
                    foreach (var (army, faces) in assignments)
                    {
                        
                        var frontlineAssgn = uncoveredSegsBefore[faces];
                        var cost = FrontlineAssignment.GetArmyAssignmentCost(army, faces, key.Data);
                        if (cost > army.MoveType(key.Data).BaseSpeed * 2f)
                        {
                            leftoverArmies.Add(army);
                        }
                        else
                        {
                            frontlineAssgn.PushArmy(army, key);
                            frontlineAssgn.ArmyFaceAssignments[faces] = army;
                        }
                        var reserveArmy = theater.GetReserveArmy(key.Data);
                        foreach (var leftoverArmy in leftoverArmies)
                        {
                            leftoverArmy.DissolveInto(reserveArmy, key);
                        }
                    }
                });
            }
            

            timer.RunChild("handling uncovered segs after", () =>
            {
                var uncoveredSegsAfter = frontlineAssgns
                    .SelectMany(fa =>
                        fa.ArmyFaceAssignments.Where(kvp => kvp.Value == null).Select(kvp => (fa, kvp.Key)))
                    .ToArray();
            
                foreach (var (fa, faces) in uncoveredSegsAfter)
                {
                    var cells = faces.Select(f => f.GetNative(key.Data)).ToHashSet();
                    var army = Army.Create(regime,
                        cells, new List<int>(), key);
                    fa.PushArmy(army, key);
                    fa.ArmyFaceAssignments[faces] = army;
                }
            });
        }
        timer.Stop();
    }

    private void TheatersPullUnits(Regime regime, LogicKey key)
    {
        var theaters = Root
            .GetDescendentNodesOfType<TheaterBranch>().ToArray();
        var armyUnits = regime.GetArmies(key.Data)
            .SelectMany(a => a.Units.Entities(key.Data)).ToHashSet();
        var freeUnits = regime.GetUnits(key.Data)
            .Except(armyUnits).ToHashSet();
        if (freeUnits.Count == 0) return;
        var theatersByUnfulfilled = theaters.ToDictionary(t => t,
            t => t.GetPowerPointNeed(key.Data));
        Assigner.AssignRanked(theaters,
            t => t.GetPowerPointNeed(key.Data),
            t => getUnits(t),
            u => u.GetPowerPoints(key.Data),
            freeUnits, 
            (t, u) => t.AddUnitToReserve(u, key),
            (t,u) => u.GetPowerPoints(key.Data));
        


        IEnumerable<Unit> getUnits(TheaterBranch t)
        {
            return t.GetDescendentAssignments()
                .SelectMany(a => a.Armies.SelectMany(army => army.Get(key.Data).Units.Entities(key.Data)));
        }


    }

    private void ClearUnusedArmies(Regime regime, LogicKey key)
    {
        var theaters = Root.GetDescendentNodesOfType<TheaterBranch>().ToArray();
        var armies = regime.GetArmies(key.Data).ToArray();
        var assignedArmies = Root.GetDescendentAssignments()
            .SelectMany(a => a.Armies)
            .Select(a => a.Get(key.Data)).ToHashSet();

        
        
        foreach (var army in armies)
        {
            if (assignedArmies.Contains(army) == false)
            {
                var theater = theaters.First(t =>
                    t.Theater.Get(key.Data).Cells.Contains(army.GetHomeCell(key.Data).MakeRef()));
                var reserveArmy = theater.GetReserveArmy(key.Data);
                army.DissolveInto(reserveArmy, key);
            }
        }
    }

}