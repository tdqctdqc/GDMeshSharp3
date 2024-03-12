
using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class ForceCompositionAi
{
    private static int PreferredGroupSize = 7;

    public ForceCompositionAi(Regime regime)
    {
        
    }

    public void Calculate(Regime regime, LogicWriteKey key)
    {
        ReinforceUnits(regime, key);
        AssignFreeUnitsToGroups(regime, key);
    }
    private void AssignFreeUnitsToGroups(Regime regime, 
        LogicWriteKey key)
    {
        var freeUnits = key.Data.Military.UnitAux.UnitByRegime[regime]
            ?.Where(u => u != null)
            .Where(u => key.Data.Military.UnitAux.UnitByGroup[u] == null)
            .ToHashSet();
        if (freeUnits == null || freeUnits.Count() == 0) return;

        var groups = key.Data.Military.UnitAux.UnitGroupByRegime[regime];
        if (groups != null)
        {
            var understrengthGroups = groups.Where(g => g.Units.Count() < PreferredGroupSize);
            foreach (var understrengthGroup in understrengthGroups)
            {
                var deficit = PreferredGroupSize - understrengthGroup.Units.Count();
                var toTake = Mathf.Min(deficit, freeUnits.Count());
                var took = freeUnits.Take(toTake);
                foreach (var unit in took)
                {
                    var proc = new SetUnitGroupProcedure(unit.MakeRef(), understrengthGroup.MakeRef());
                    freeUnits.Remove(unit);
                    key.SendMessage(proc);
                }
            }
        }
        
        
        
        var numNewGroups = Mathf.CeilToInt((float)freeUnits.Count() / PreferredGroupSize);
        if (numNewGroups == 0) return;
        var newGroups = Enumerable.Range(0, numNewGroups)
            .Select(i => new List<int>())
            .ToList();
        
        var iter = 0;
        foreach (var freeUnit in freeUnits)
        {
            var group = iter % numNewGroups;
            key.Data.Logger.Log($"adding unit to group pre", LogType.Temp);

            newGroups.ElementAt(group).Add(freeUnit.Id);
            iter++;
        }
        foreach (var newGroup in newGroups)
        {
            if (newGroup.Count() == 0) continue;
            key.Data.Logger.Log($"creating new group from {newGroup.Count()} units", LogType.Temp);
            UnitGroup.Create(regime,
                newGroup, key);
        }
    }
    private void ReinforceUnits(Regime regime,
        LogicWriteKey key)
    {
        var needCounts = new Dictionary<Troop, float>();
        foreach (var unit in regime.GetUnits(key.Data))
        {
            var template = unit.Template.Get(key.Data);
            foreach (var (troop, value) in unit.Troops.GetEnumerableModel(key.Data))
            {
                var shouldHave = template.TroopCounts.Get(troop);
                if (value < shouldHave)
                {
                    needCounts.AddOrSum(troop, shouldHave - value);
                }
            }
        }
        
        var proc = ReinforceUnitProcedure.Construct(regime);
        var reserve = regime.Store;
        foreach (var unit in regime.GetUnits(key.Data))
        {
            var template = unit.Template.Get(key.Data);
            foreach (var (troop, value) in unit.Troops.GetEnumerableModel(key.Data))
            {
                if (needCounts.ContainsKey(troop) == false) continue;
                if (reserve.Contents.ContainsKey(troop.Id) == false) continue;
                var shouldHave = template.TroopCounts.Get(troop);
                if (value < shouldHave)
                {
                    var need = shouldHave - value;
                    var receiveRatio = reserve.Get(troop) / needCounts[troop];
                    receiveRatio = Mathf.Clamp(receiveRatio, 0f, 1f);
                    proc.ReinforceCounts.Add((unit.Id, troop.Id, need * receiveRatio));
                }
            }
        }
        key.SendMessage(proc);
    }
}