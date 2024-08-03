
using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class ForceCompositionAi
{
    private static int PreferredGroupSize = 7;
    public Dictionary<UnitTemplatesAi.UnitTypeTag, int> DesiredAmounts { get; private set; }

    public ForceCompositionAi(Dictionary<UnitTemplatesAi.UnitTypeTag, int> desiredAmounts)
    {
        DesiredAmounts = desiredAmounts;
    }


    public void Calculate(Regime regime, LogicKey key)
    {
        CalcDesired(regime, key);
        ReinforceUnits(regime, key);
        AssignFreeUnitsToGroups(regime, key);
    }

    private void CalcDesired(Regime regime, LogicKey key)
    {
        var templatesAi = key.Data.HostLogicData.RegimeAis[regime]
            .Military.Templates;
        var weights = new Dictionary<UnitTemplatesAi.UnitTypeTag, float>
        {
            {UnitTemplatesAi.UnitTypeTag.Infantry, 1f}
        };
        //do mods

        var totalWeight = weights.Sum(kvp => kvp.Value);
        
        var numCells = regime.GetCells(key.Data).Count();
        var approxBorder = Mathf.Sqrt(numCells) * 2.5f;
        var unitsPerBorder = 2f / UnitTemplatesAi.SingleUnitFrontProportion;
        var numUnits = approxBorder * unitsPerBorder;
        DesiredAmounts = weights.ToDictionary(kvp => kvp.Key,
            kvp => Mathf.CeilToInt(numUnits * kvp.Value / totalWeight));
    }

    public Dictionary<UnitMetaTemplate, Vector2I> 
        GetCurrentAndNeededTotals(Regime r, Data d)
    {
        var metas = d.HostLogicData.RegimeAis[r].Military.Templates
            .MetaTemplates;
        var unitsByMeta = 
            r.GetUnits(d)
                .SortBy(u => u.Template.Get(d).GetMetaTemplate(d));
        var needed = DesiredAmounts.ToDictionary(kvp => kvp.Key,
            kvp => unitsByMeta.TryGetValue( metas[kvp.Key], out var list)
                ? Mathf.Max(0, kvp.Value - list.Count)
                : kvp.Value);
        
        return unitsByMeta.ToDictionary(kvp => kvp.Key,
            kvp => new Vector2I(needed[kvp.Key.Tag], unitsByMeta[kvp.Key].Count()));
    }
    private void AssignFreeUnitsToGroups(Regime regime, 
        LogicKey key)
    {
        var freeUnits = regime.GetUnits(key.Data)
            ?.Where(u => u != null)
            .Where(u => key.Data.Military.UnitAux.UnitByGroup[u] == null)
            .ToHashSet();
        if (freeUnits == null || freeUnits.Any() == false) return;
        
                
        var groups = key.Data.GetAll<Army>()
            .Where(g => g.Regime.RefId == regime.Id)?.ToArray();
        if (groups is not null && groups.Length > 0)
        {
            var understrengthGroups = groups.Where(g => g.Units.Count() < PreferredGroupSize);
            foreach (var understrengthGroup in understrengthGroups)
            {
                var deficit = PreferredGroupSize - understrengthGroup.Units.Count();
                var toTake = Mathf.Min(deficit, freeUnits.Count());
                var took = freeUnits.Take(toTake);
                foreach (var unit in took)
                {
                    var proc = new SetUnitArmyProcedure(unit.MakeRef(), understrengthGroup.MakeRef());
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
            if (newGroup.Count == 0) continue;
            key.Data.Logger.Log($"creating new group from {newGroup.Count()} units", LogType.Temp);
            Army.Create(
                regime, 
                regime.Capital.Get(key.Data).Yield(),
                newGroup, key);
        }
    }
    private void ReinforceUnits(Regime regime,
        LogicKey key)
    {
        var needCounts = new Dictionary<TroopType, float>();
        var units = regime.GetUnits(key.Data);
        if (units == null) return;
        foreach (var unit in units)
        {
            var template = unit.Template.Get(key.Data);
            foreach (var (troop, value) in unit.Troops.GetEnumModel(key.Data)
                         .SortInto(kvp => kvp.Key.TroopType, kvp => kvp.Value))
            {
                var shouldHave = template.Troops.Get(troop);
                if (value < shouldHave)
                {
                    needCounts.AddOrSum(troop, shouldHave - value);
                }
            }
        }
        
        var proc = ReinforceProcedure.Construct(regime);
        var reserve = regime.Stock;

        var reservesByType = new Dictionary<TroopType, List<Troop>>();
        var reservesRemaining = new Dictionary<Troop, float>();
        foreach (var (model, value) in regime.Stock.Stock.GetEnumModel(key.Data))
        {
            if (model is not Troop t) continue;
            if(reservesByType.ContainsKey(t.TroopType) == false)
            {
                reservesByType.Add(t.TroopType, new List<Troop>());
            }

            reservesByType[t.TroopType].Add(t);
            reservesRemaining.Add(t, value);
        }
        foreach (var (troopType, value) in reservesByType)
        {
            value.Sort((t1, t2) =>
            {
                return Mathf.FloorToInt(t2.GetPowerPoints() - t1.GetPowerPoints());
            });
        }
        
        foreach (var unit in regime.GetUnits(key.Data))
        {
            var template = unit.Template.Get(key.Data);
            foreach (var (troopType, value) in unit.Troops.GetEnumModel(key.Data)
                         .SortInto(kvp => kvp.Key.TroopType, kvp => kvp.Value))
            {
                if (needCounts.ContainsKey(troopType) == false) continue;
                if (reservesByType.ContainsKey(troopType) == false) continue;
                var shouldHave = template.Troops.Get(troopType);
                if (value < shouldHave)
                {
                    var need = shouldHave - value;
                    
                    foreach (var troop in reservesByType[troopType])
                    {
                        if (need == 0f) break;
                        if (reservesRemaining[troop] == 0f) continue;
                        var take = Mathf.Min(need, reservesRemaining[troop]);
                        reservesRemaining[troop] -= take;
                        proc.ReinforceCounts.Add((unit.Id, troop.Id, take));
                    }
                }
            }
        }
        key.SendMessage(proc);
    }
}