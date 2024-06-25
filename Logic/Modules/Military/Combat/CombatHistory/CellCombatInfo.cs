//
// using System.Collections.Generic;
// using System.Linq;
// using Godot;
//
// public class CellCombatInfo
// {
//     public int CellId { get; private set; }
//     public ERef<Unit>[] AttackerUnits { get; private set; }
//     public IdCount<Troop>[] AttackerTroops { get; set; }
//     public IdCount<Troop>[] AttackerFrontTroops { get; set; }
//     public IdCount<Troop>[] AttackerLosses { get; private set; }
//     public IdCount<Troop>[] AttackerKills { get; set; }
//     public ERef<Unit>[] DefenderUnits { get; private set; }
//     public IdCount<Troop>[] DefenderTroops { get; set; }
//     public IdCount<Troop>[] DefenderFrontTroops { get; set; }
//     public IdCount<Troop>[] DefenderLosses { get; private set; }
//     public IdCount<Troop>[] DefenderKills { get; set; }
//     public bool ForcedBack { get; private set; }
//     public static CellCombatInfo Construct(CellDefenseNode node,
//         CombatCalculator combat, Data d)
//     {
//         var atkEdges = combat.Graph
//             .GetNodeEdges(node)
//             .OfType<CellAttackEdge>();
//         var attackingUnits = new List<Unit>();
//         var attackingTroopsByUnit = new List<IdCount<Troop>>();
//         
//         foreach (var edge in atkEdges)
//         {
//             foreach (var (unit, prop) in edge.Attackers)
//             {
//                 attackingUnits.Add(unit);
//                 var count = IdCount<Troop>.Construct();
//                 foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(d))
//                 {
//                     count.Add(troop, amt * prop);
//                 }
//                 attackingTroopsByUnit.Add(count);
//             }
//         }
//         
//         
//         var defEdges = combat.Graph.GetNodeEdges(node)
//             .OfType<ArmyDefendEdge>();
//         var defendingUnits = new List<Unit>();
//         var defendingTroopsByUnit = new List<IdCount<Troop>>();
//         foreach (var edge in defEdges)
//         {
//             foreach (var (unit, prop) in edge.Defenders)
//             {
//                 defendingUnits.Add(unit);
//                 var count = IdCount<Troop>.Construct();
//                 var frontCount = IdCount<Troop>.Construct();
//                 foreach (var (troop, amt) in unit.Troops.GetEnumerableModel(d))
//                 {
//                     count.Add(troop, amt * prop);
//                 }
//                 defendingTroopsByUnit.Add(count);
//             }
//         }
//         
//         
//         var frontLength = CellDefenseNode.BaseFrontLength
//                           * node.Cell.GetLandform(d).FrontLengthMult
//                           * node.Cell.GetVegetation(d).FrontLengthMult;
//         var totalDefFrontage = defendingTroopsByUnit
//             .Sum(count => count.GetEnumerableModel(d)
//                 .Sum(kvp => kvp.Key.FrontLength * kvp.Value));
//         var defDeployedRatio = frontLength / totalDefFrontage;
//         defDeployedRatio = Mathf.Clamp(0f, 1f, defDeployedRatio);
//         var defendingFrontTroopsByUnit = new List<IdCount<Troop>>();
//         for (var i = 0; i < defendingTroopsByUnit.Count; i++)
//         {
//             var count = IdCount<Troop>.Construct();
//
//             foreach (var (troop, amt) in defendingFrontTroopsByUnit[i].GetEnumerableModel(d))
//             {
//                 count.Add(troop, amt * defDeployedRatio);
//             }
//             defendingFrontTroopsByUnit.Add(count);
//         }
//         
//         
//         var totalAtkFrontage = attackingTroopsByUnit
//             .Sum(count => count.GetEnumerableModel(d)
//                 .Sum(kvp => kvp.Key.FrontLength * kvp.Value));
//         var atkDeployedRatio = frontLength / totalAtkFrontage;
//         atkDeployedRatio = Mathf.Clamp(0f, 1f, atkDeployedRatio);
//         var attackingFrontTroopsByUnit = new List<IdCount<Troop>>();
//         for (var i = 0; i < attackingTroopsByUnit.Count; i++)
//         {
//             var count = IdCount<Troop>.Construct();
//
//             foreach (var (troop, amt) in attackingFrontTroopsByUnit[i].GetEnumerableModel(d))
//             {
//                 count.Add(troop, amt * atkDeployedRatio);
//             }
//             attackingFrontTroopsByUnit.Add(count);
//         }
//         
//         
//         
//         return new CellCombatInfo(
//             node.Cell.Id,
//             attackingUnits.Select(u => u.MakeRef()).ToArray(),
//             attackingTroopsByUnit.ToArray(), 
//             attackingFrontTroopsByUnit.ToArray(), 
//             attackingUnits.Select(d => IdCount<Troop>.Construct()).ToArray(),
//             attackingUnits.Select(d => IdCount<Troop>.Construct()).ToArray(),
//             defendingUnits.Select(u => u.MakeRef()).ToArray(), 
//             defendingTroopsByUnit.ToArray(),
//             defendingFrontTroopsByUnit.ToArray(),
//             defendingUnits.Select(d => IdCount<Troop>.Construct()).ToArray(),
//             defendingUnits.Select(d => IdCount<Troop>.Construct()).ToArray(),
//             node.DefendersForcedBack);
//     }
//     public CellCombatInfo(
//         int cellId,
//         ERef<Unit>[] attackerUnits, 
//         IdCount<Troop>[] attackerTroops, 
//         IdCount<Troop>[] attackerFrontTroops, 
//         IdCount<Troop>[] attackerLosses, 
//         IdCount<Troop>[] attackerKills, 
//         ERef<Unit>[] defenderUnits, 
//         IdCount<Troop>[] defenderTroops, 
//         IdCount<Troop>[] defenderFrontTroops, 
//         IdCount<Troop>[] defenderLosses,
//         IdCount<Troop>[] defenderKills,
//         bool forcedBack)
//     {
//         CellId = cellId;
//         AttackerUnits = attackerUnits;
//         AttackerTroops = attackerTroops;
//         AttackerFrontTroops = attackerFrontTroops;
//         AttackerLosses = attackerLosses;
//         AttackerKills = attackerKills;
//         DefenderUnits = defenderUnits;
//         DefenderTroops = defenderTroops;
//         DefenderFrontTroops = defenderFrontTroops;
//         DefenderLosses = defenderLosses;
//         DefenderKills = defenderKills;
//         ForcedBack = forcedBack;
//     }
//
//     public void AddLossesAndKills(
//         Unit attacker, float attackerProportionLosses,
//         Unit defender, float defenderProportionLosses)
//     {
//         var attackerIndex = AttackerUnits.IndexOf(attacker.MakeRef());
//         var defenderIndex = DefenderUnits.IndexOf(defender.MakeRef());
//         
//         foreach (var (troop, num) 
//                  in AttackerTroops[attackerIndex].Contents)
//         {
//             var loss = num * attackerProportionLosses;
//             AttackerLosses[attackerIndex].Add(troop, loss);
//             DefenderKills[defenderIndex].Add(troop, loss);
//         }
//         
//         foreach (var (troop, num) 
//                  in DefenderTroops[defenderIndex].Contents)
//         {
//             var loss = num * defenderProportionLosses;
//             AttackerKills[attackerIndex].Add(troop, loss);
//             DefenderLosses[defenderIndex].Add(troop, loss);
//         }
//     }
// }