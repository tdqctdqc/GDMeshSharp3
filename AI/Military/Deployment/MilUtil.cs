
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MilUtil
{
    public static float CoverOpposingWeight { get; private set; }
        = .5f;
    public static float CoverLengthWeight { get; private set; }
        = 1f;
    public static float DesiredOpposingPpRatio { get; private set; }
        = 2f;
    public static float PowerPointsPerCellFaceToCover { get; private set; }
        = 100f;
    public static float LossRatioToForceBack { get; private set; } 
        = .3f;
    public static int BaseFrontLength { get; private set; }
        = 1000;

    public static bool CalculateCombat(
        UnitCombatInfo[] attackers,
        UnitCombatInfo[] defenders,
        Landform lf, Vegetation veg, Data d)
    {
        foreach (var unitCombatInfo in attackers)
        {
            doFights(unitCombatInfo, defenders, attackers);
        }
        
        void doFights(UnitCombatInfo unit, 
            UnitCombatInfo[] targets,
            UnitCombatInfo[] comrades)
        {
            foreach (var (troop, amt) in unit.Active.GetEnumModel(d))
            {
                var ceil = Mathf.CeilToInt(amt);
                for (var i = 0; i < ceil; i++)
                {
                    var targetEchelon = getTargetEchelon(troop, targets, comrades);
                    if (targetEchelon == -1) return;
                    var targetUnit = getTargetUnit(targets, targetEchelon);
                    if (targetUnit == null) return;
                    var (targetTroop, targetTroopAmt) = getTargetTroop(targetUnit,
                        targetEchelon);
                    if (targetTroop == null) return;
                    
                    if (getHit(troop, targetTroop, true))
                    {
                        var kill = getKillAmt(troop, 
                            targetTroop, targetTroopAmt);
                        unit.AddKill(targetTroop, kill);
                        targetUnit.AddLoss(targetTroop, kill);
                    }
                    
                    if (getHit(targetTroop, troop, false))
                    {
                        var kill = getKillAmt(targetTroop, troop, 
                            unit.Active.Get(targetTroop));
                        unit.AddLoss(troop, kill);
                        targetUnit.AddKill(troop, kill);
                    }
                }
            }
        }
        int getTargetEchelon(Troop troop, UnitCombatInfo[] targets,
            UnitCombatInfo[] comrades)
        {
            if (targets.Length == 0) return -1;
            if (targets.Any(t => t.ActiveFrontSize > 0f)
                == false)
            {
                return -1;
            }
            if (troop.Range >= troop.TargetEchelon
                && targets.Sum(t => 
                    t.GetEchelonFrontage(troop.TargetEchelon, d))
                > 0f)
            {
                return troop.TargetEchelon;
            }

            var maxEchelon = targets
                .Where(t => t.ActiveFrontSize > 0f)
                .Max(t => t.GetMaxEchelon(d));
            var minEchelon = targets
                .Where(t => t.ActiveFrontSize > 0f)
                .Min(t => t.GetMinEchelon(d));

            var start = Mathf.Clamp(troop.Range, minEchelon, maxEchelon);
            
            var carryOver = 0f;
            var carryMult = .5f;
            for (var i = 0; i <= maxEchelon; i++)
            {
                var enemyFrontage = targets.Sum(c => c.GetEchelonFrontage(i, d));
                if (enemyFrontage == 0f) continue;
                
                if (i == troop.TargetEchelon || i == maxEchelon)
                {
                    return i;
                }
                
                var frontage = comrades.Sum(c => c.GetEchelonFrontage(i, d));
                frontage += carryOver;
                
                carryOver = Mathf.Max(0f, (frontage - enemyFrontage) * carryOver);

                var breakthrough = Game.I.Random.RandfRange(0f, frontage * troop.BreakthroughMult);
                if (breakthrough > enemyFrontage)
                {
                    continue;
                }

                return i;
            }

            return -1;
        }
        bool getHit(Troop troop, Troop target, bool targetIsDefense)
        {
            var toHit = Random.Shared.NextSingle()
                        * troop.Accuracy;
            var evadeMult = GetEvasionMult(lf, veg, targetIsDefense);
            var toEvade = Random.Shared.NextSingle()
                          * target.Evasion * evadeMult;
            return toHit > toEvade;
        }

        float getKillAmt(Troop troop, Troop target, float targetAmt)
        {
            var dmg = getDamage(troop, target);
            return Mathf.Min(targetAmt, dmg / target.Hitpoints);
        }
        float getDamage(Troop troop, Troop target)
        {
            var softDmg = troop.SoftAttack * (1f - target.Hardness);
            var hardDmg = troop.HardAttack * target.Hardness;
            return softDmg + hardDmg;
        }

        
        UnitCombatInfo getTargetUnit(UnitCombatInfo[] targets, 
            int echelon)
        {
            var totalLength = targets
                .Sum(t => t.GetEchelonFrontage(echelon, d));
            if (totalLength == 0f) return null;
            var s = Game.I.Random.RandfRange(0, totalLength - .01f);
            
            var i = 0;
            while (s > targets[i].GetEchelonFrontage(echelon, d))
            {
                s -= targets[i].GetEchelonFrontage(echelon, d);
                i++;
            }

            return targets[i];
        }

        (Troop troop, float amt) getTargetTroop(UnitCombatInfo targetUnit,
            int targetEchelon)
        {
            var echelonFrontage = targetUnit.GetEchelonFrontage(targetEchelon, d);
            if (echelonFrontage <= 0f) return (null, 0f);
            var sample = Game.I.Random.RandfRange(0f, echelonFrontage);
            var soFar = 0f;
            foreach (var (troop, amt) in targetUnit.Active.GetEnumModel(d))
            {
                soFar += amt * troop.FrontLength;
                if (soFar >= sample - .01f) return (troop, Mathf.Min(1f, amt));
            }

            throw new Exception();
        }

        if (defenders.All(info => info.ActiveFrontSize <= 0f))
        {
            return true;
        }

        var defPower = defenders.Sum(i => i.InitialPowerPoints(d));
        var lostDefPower = defenders.Sum(i => i.LostPowerPoints(d));
        var defLossRatio = lostDefPower / defPower;
        if (lostDefPower / defPower >= LossRatioToForceBack)
        {
            var atkPower = attackers.Sum(i => i.InitialPowerPoints(d));
            var lostAtkPower = attackers.Sum(i => i.LostPowerPoints(d));
            var atkLossRatio = lostAtkPower / atkPower;
            if (atkLossRatio < defLossRatio)
            {
                return true;
            }
        }

        return false;
    }
    
    public static Dictionary<Army, HashSet<Cell>> 
        GetGroupLineAssignments(Alliance alliance,
            IEnumerable<Army> groups,
            List<FrontFace> faces,
            Func<FrontFace, float> getFaceCost,
            Data d)
    {
        var groupsInOrder = GetLineGroupsInOrder(faces,
            groups, d);
        var lineOrders = Assigner
            .PickInOrderAndAssignAlongFaces2(
            faces, 
            groupsInOrder, 
            u => u.GetPowerPoints(d),
            getFaceCost);
        return lineOrders.ToDictionary(kvp => kvp.Key,
            kvp => faces.GetRange(kvp.Value.X, kvp.Value.Y - kvp.Value.X + 1)
                .Select(f => f.GetNative(d)).ToHashSet());
    }
    public static List<Army> GetLineGroupsInOrder(List<FrontFace> faces,
        IEnumerable<Army> lineGroups,
        Data d)
    {
        var list = lineGroups.ToList();
        list.Sort((g, f) =>
        {
            var boundsG = g.GetCells(d);
            var gFirst = faces
                .FindIndex(f => boundsG.Contains(f.GetNative(d)));
            var gLast = faces
                .FindLastIndex(f => boundsG.Contains(f.GetNative(d)));

            var boundsF = f.GetCells(d);
            var fFirst = faces
                .FindIndex(f => boundsF.Contains(f.GetNative(d)));
            var fLast = faces
                .FindLastIndex(f => boundsF.Contains(f.GetNative(d)));

            if (gFirst == -1 || gLast == -1 || fFirst == -1 || fLast == -1)
            {
                return 0;
            }
            if (gFirst < fFirst) return -1;
            if (fFirst < gFirst) return 1;
            if (gLast < fLast) return -1;
            if (fLast < gLast) return 1;
            return 0;
        });
        return list;
    }



    public static float GetEvasionMult(Landform lf, Vegetation veg, bool defending)
    {
        var evadeMult = lf.EvasionMult * veg.EvasionMult;
        if (defending == false)
        {
            evadeMult *= .5f;
            evadeMult = Mathf.Max(1f, evadeMult);
        }

        return evadeMult;
    }
}