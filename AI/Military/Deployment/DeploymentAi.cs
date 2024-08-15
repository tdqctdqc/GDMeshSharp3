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
        var stratAi = alliance.GetAi(key.Data).Military.Strategic;
        var oldFrontlineGroupAssignments = 
            Root
            ?.GetDescendentAssignmentsOfType<FrontlineAssignment>()
            .ToDictionary(v => v.Frontline,
                v => v.Armies.ToHashSet());
        var oldTheaterAssignments = 
            Root
            ?.GetDescendentNodesOfType<TheaterBranch>()
                .ToArray();
        var milAi = alliance.GetAi(key.Data).Military;
        Clear(key);
        Root = new DeploymentRoot(alliance.MakeRef(),
            key.Data.IdDispenser.TakeId(),
            new HashSet<DeploymentBranch>(), 
            new HashSet<ArmyAssignment>());
        
        Root.MakeTheaters(milAi, key);
        Root.MergeToNew(Root, stratAi.Context, key);
        PatchFrontlines(key);
        Root.SetWeights(key);
        Root.ShiftUnits(key);
        Root.GiveOrders(key);
    }
    

    private void PatchFrontlines(LogicKey key)
    {
        var leader = Alliance.Get(key.Data).Leader.Get(key.Data);
        var theaters = Root
            .GetDescendentNodesOfType<TheaterBranch>()
            .ToArray();
        
        
        foreach (var theater in theaters)
        {
            var frontlineAssgns 
                = theater.GetDescendentAssignmentsOfType<FrontlineAssignment>();
            foreach (var fa in frontlineAssgns)
            {
                fa.SetupFrontSegments(key);
            }
            var freeArmies = frontlineAssgns
                .SelectMany(fa => fa.AssignArmiesToSegs(key)).ToHashSet();
            if (freeArmies.Count > 0)
            {
                var uncoveredSegsBefore = frontlineAssgns
                    .SelectMany(fa =>
                        fa.ArmyFaceAssignments.Where(kvp => kvp.Value == null).Select(kvp => (fa, kvp.Key)))
                    .ToArray();
            
                var assignments = OrToolsExt.GetAssignment(
                    freeArmies.ToList(), uncoveredSegsBefore, 
                    (a, f) => (int)a.GetMoveCost(f.Key.GetMiddleElement().GetNative(key.Data), key.Data),
                    out var leftoverArmies, 
                    out var leftoverSegs, 
                    out var costs);
            
                foreach (var (army, v) in assignments)
                {
                    var (frontlineAssgn, faces) = v;
                    var cost = costs[(army, v)];
                    if (cost > army.MoveType(key.Data).BaseSpeed * 2f)
                    {
                        leftoverArmies.Add(army);
                    }
                    else
                    {
                        frontlineAssgn.PushArmy(army, key);
                        frontlineAssgn.ArmyFaceAssignments[faces] = army;
                    }
                }
            }
            
            
            var uncoveredSegsAfter = frontlineAssgns
                .SelectMany(fa =>
                    fa.ArmyFaceAssignments.Where(kvp => kvp.Value == null).Select(kvp => (fa, kvp.Key)))
                .ToArray();
            
            foreach (var (fa, faces) in uncoveredSegsAfter)
            {

                var cells = faces.Select(f => f.GetNative(key.Data)).ToHashSet();
                var army = Army.Create(leader,
                    cells, new List<int>(), key);
                fa.PushArmy(army, key);
                fa.ArmyFaceAssignments[faces] = army;
            }
        }
    }

    private void TheatersPullUnits(LogicKey key)
    {
        // var theaters = Root.GetDescendentNodesOfType<TheaterBranch>();
        // var freeUnits = Alliance.Get(key.Data).
    }

    public DeploymentRoot GetRoot()
    {
        return Root;
    }
}