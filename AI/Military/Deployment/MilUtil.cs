
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MilUtil
{
    public static int NumEchelons { get; private set; }
        = 4;
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

    public static float DefenderMinMorale { get; private set; }
        = .25f;
    public static float AttackerMinMorale { get; private set; }
        = .5f;
    public static int BaseFrontLength { get; private set; }
        = 1000;
    

    public static bool CalculateCombat(
        UnitCombatInfo[] attackers,
        UnitCombatInfo[] defenders,
        Landform lf, Vegetation veg, Data d)
    {
        var attackersStill = attackers
            .Where(u => u.Morale >= AttackerMinMorale).ToArray();
        if (attackersStill.Length == 0) return false;
        
        var defendersStill = defenders
            .Where(u => u.Morale >= DefenderMinMorale).ToArray();
        if (defendersStill.Length == 0) return true;
        
        foreach (var unitCombatInfo in defenders)
        {
            doFights(unitCombatInfo, attackers, 
                defenders, false);
        }

        attackersStill = attackers
            .Where(u => u.Morale >= AttackerMinMorale).ToArray();
        if (attackersStill.Length == 0) return false;
        
        defendersStill = defenders
            .Where(u => u.Morale >= DefenderMinMorale).ToArray();
        if (defendersStill.Length == 0) return true;

        foreach (var unitCombatInfo in attackersStill)
        {
            doFights(unitCombatInfo, defendersStill, 
                attackersStill, true);
        }
        
        if (attackersStill.All(info => info.ActiveFrontSizes.Sum() <= 0f))
        {
            return false;
        }
        if (defendersStill.All(info => info.ActiveFrontSizes.Sum() <= 0f))
        {
            return true;
        }
        
        attackersStill = attackers
            .Where(u => u.Morale >= AttackerMinMorale).ToArray();
        if (attackersStill.Length == 0) return false;
        
        defendersStill = defenders
            .Where(u => u.Morale >= DefenderMinMorale).ToArray();
        if (defendersStill.Length == 0) return true;

        return false;
        
        
        
        
        void doFights(UnitCombatInfo unit, 
            UnitCombatInfo[] targets,
            UnitCombatInfo[] comrades,
            bool targetIsDefendingCell)
        {
            foreach (var (troop, amt) in unit.Active.GetEnumModel(d))
            {
                var ceil = Mathf.CeilToInt(amt);
                for (var i = 0; i < ceil; i++)
                {
                    var troopAmt = Mathf.Min(1f, unit.Active.Get(troop));
                    var targetEchelon = GetTargetEchelon(troop, lf, veg, 
                        targets, comrades);
                    if (targetEchelon == -1)
                    {
                        return;
                    }
                    var targetUnit = getTargetUnit(targets, targetEchelon);
                    if (targetUnit == null)
                    {
                        return;
                    }
                    var (targetTroop, targetTroopAmt) = getTargetTroop(targetUnit,
                        targetEchelon);
                    if (targetTroop == null)
                    {
                        return;
                    }
                    
                    if (getHit(troop, targetTroop, 
                            targetIsDefendingCell, true))
                    {
                        var kill = getKillAmt(troop, 
                            targetTroop, targetTroopAmt);
                        unit.AddKill(targetTroop, kill);
                        targetUnit.AddLoss(targetTroop, kill);
                    }
                    
                    if (getHit(targetTroop, troop, 
                            targetIsDefendingCell == false, false))
                    {
                        var kill = getKillAmt(targetTroop, troop, 
                            troopAmt);
                        unit.AddLoss(troop, kill);
                        targetUnit.AddKill(troop, kill);
                    }
                }
            }
        }
        
        bool getHit(Troop troop, Troop target, 
            bool targetIsDefendingCell, bool troopInitiated)
        {
            var toHit = Random.Shared.NextSingle()
                        * troop.Accuracy;
            if (troopInitiated == false && target.Range > troop.Range)
            {
                //shields 'bombard' from return fire
                return false;
            }
            var evadeMult = GetEvasionMult(lf, veg, targetIsDefendingCell);
            var toEvade = Random.Shared.NextSingle()
                          * target.Evasion * evadeMult;
            
            return toHit > toEvade;
        }

        float getKillAmt(Troop troop, Troop target, 
            float targetAmt)
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
                .Sum(t => t.ActiveFrontSizes[echelon]);
            if (totalLength == 0f)
            {
                return null;
            }
            var s = Game.I.Random.RandfRange(0, totalLength - .01f);
            
            var i = 0;
            while (s > targets[i].ActiveFrontSizes[echelon])
            {
                s -= targets[i].ActiveFrontSizes[echelon];
                i++;
            }

            return targets[i];
        }

        (Troop troop, float amt) getTargetTroop(UnitCombatInfo targetUnit,
            int targetEchelon)
        {
            var echelonFrontage = targetUnit.ActiveFrontSizes[targetEchelon];
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

        
    }
    
    public static int GetTargetEchelon(Troop troop, 
        Landform lf, Vegetation veg,
        UnitCombatInfo[] targets,
        UnitCombatInfo[] friendlies)
    {
        if (targets.Length == 0) return -1;
        if (targets.Any(t => t.ActiveFrontSizes.Sum() > 0f)
            == false)
        {
            return -1;
        }

        var carryOver = 0f;
        var carryMult = .5f;
        
        var chances = GetEchelonChances(troop, lf, veg, targets, friendlies);
        
        var totalChance = chances.Sum();
        var score = Game.I.Random.RandfRange(0f, totalChance);
        var cumul = 0f;
        for (var i = 0; i < chances.Length; i++)
        {
            cumul += chances[i];
            if (cumul >= score)
            {
                return i;
            }
        }

        throw new Exception();
    }

    public static float[] GetEchelonChances(Troop troop, 
        Landform lf, Vegetation veg, 
        UnitCombatInfo[] targets,
        UnitCombatInfo[] friendlies)
    {
        var carryOver = 0f;
        var carryMult = .5f;
        var chances = new float[NumEchelons];
        for (int i = 0; i < NumEchelons; i++)
        {
            var enemyFrontage = targets.Sum(c => c.ActiveFrontSizes[i]);
            var friendlyFrontage = friendlies.Sum(c => c.ActiveFrontSizes[i]);
            var chance = 0f;
            if (enemyFrontage > 0f)
            {
                var baseChance = troop.TargetChance[i];
                chance = baseChance * enemyFrontage;
                if (i > troop.Range)
                {
                    var movementMult = lf.MovementCostMult * veg.MovementCostMult;
                    
                    var frontageModTop = (friendlyFrontage + carryOver) * troop.BreakthroughMult;
                    var frontageModBottom = (enemyFrontage * movementMult);
                    
                    var frontageMod = frontageModTop / frontageModBottom;
                    frontageMod = Mathf.Clamp(frontageMod, 0f, 1f);
                    chance *= frontageMod;
                }
            }

            carryOver = Mathf.Max(0f, (friendlyFrontage + carryOver - enemyFrontage) * carryMult);
            chances[i] = chance;
        }

        return chances;
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
            a => a.GetPowerPoints(d),
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



    public static float GetEvasionMult(Landform lf, 
        Vegetation veg, bool defending)
    {
        var evadeMult = lf.EvasionMult * veg.EvasionMult;
        if (defending == false)
        {
            evadeMult *= .5f;
            // evadeMult = Mathf.Max(1f, evadeMult);
        }

        return evadeMult;
    }
}