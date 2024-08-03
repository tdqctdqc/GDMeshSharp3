
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UpgradeTroopsPriority : IBudgetPriority
{
    public string Name { get; private set; }

    public UpgradeTroopsPriority()
    {
        Name = "Upgrade Troops";
    }

    public Dictionary<IModel, float> GetWishlist(Regime regime, Data d)
    {
        return GetNeeded(regime, d).ToDictionary(kvp => (IModel)kvp.Key, kvp => kvp.Value);
    }

    public Dictionary<IModel, float> GetWishlistCosts(Regime regime,
        Data d)
    {
        var res = new Dictionary<IModel, float>();
        var needed = GetNeeded(regime, d);
        foreach (var (troop, value) in needed)
        {
            foreach (var (item, amt) in troop.Makeable.BuildCosts.GetEnumModel(d))
            {
                res.AddOrSum(item, amt * value);
            }
        }
        return res;
    }

    public void Calculate(BudgetPool pool, Regime regime, 
        LogicKey key, out Dictionary<IModel, float> modelCosts,
        out Dictionary<string, float> built)
    {
        var pool2 = new BudgetPool(
            IdCount<IModel>.Construct(pool.Stock),
            IdCount<IModel>.Construct(pool.Net));
        modelCosts = new Dictionary<IModel, float>();
        
        var d = key.Data;
        var needed = GetNeeded(regime, d);
        var best = d.Models.GetModels<TroopType>()
            .ToDictionary(tt => tt, tt => regime.Military.GetBestTroopOfType(tt, d));

        var toBuild = new Dictionary<Troop, float>();
        foreach (var (troop, troopAmt) in needed)
        {
            var troopBest = best[troop.TroopType];
            if(troopBest == troop) continue;
            var troopUpgradeCosts = IdCount<Item>.Construct(troopBest.Makeable.BuildCosts);
        
            foreach (var (buildItem, buildItemAmt) in troop.Makeable.BuildCosts.GetEnumModel(d))
            {
                troopUpgradeCosts.Remove(buildItem, Mathf.Min(buildItemAmt, 
                    troopUpgradeCosts.Get(buildItem)));
            }

            var ratio = 1f;
            foreach (var (buildItem, buildItemAmt) in troopUpgradeCosts.GetEnumModel(d))
            {
                var stock = pool2.Stock.Get(buildItem);
                var neededStock = buildItemAmt * troopAmt;
                var thisRatio = Mathf.Clamp(stock / neededStock, 0f, 1f);
                ratio = Mathf.Min(thisRatio, ratio);
                if (ratio == 0f) break;
            }

            if (ratio == 0f) continue;

            var buildAmt = troopAmt * ratio;
            
            foreach (var (buildItem, value) in troopUpgradeCosts.GetEnumModel(d))
            {
                pool2.Stock.Remove(buildItem, value * buildAmt);
                modelCosts.AddOrSum(buildItem, value);
            }
            
            
            toBuild.Add(troop, buildAmt);
            var newProj = TroopUpgradeProject.Construct(
                regime, troop,
                troopBest,
                buildAmt, key.Data);
            var proc = new StartOrConsolidateMakeProject(newProj);
            key.SendMessage(proc);
        }
        
        
        
        
        foreach (var (from, value) in toBuild)
        {
            var remaining = value;
            var to = best[from.TroopType];
            foreach (var unit in regime.GetUnits(key.Data).ToArray())
            {
                if (remaining <= 0f) break;
                if (unit.Troops.Get(from) == 0f)
                {
                    continue;
                }
        
                var unitAmt = unit.Troops.Get(from);
                unitAmt = Mathf.Min(unitAmt, remaining);
                remaining -= unitAmt;
                var newProj = TroopUpgradeProject.Construct(
                    regime, from,
                    to,
                    unitAmt, key.Data,
                    unit);
                var proc = new StartOrConsolidateMakeProject(newProj);
                key.SendMessage(proc);
            }
        
            if (remaining > 0f)
            {
                var inStock = regime.Stock.Stock.Get(from);
                var amt = Mathf.Min(inStock, remaining);
                var newProj = TroopUpgradeProject.Construct(
                    regime, from,
                    to,
                    amt, key.Data);
                var proc = new StartOrConsolidateMakeProject(newProj);
                key.SendMessage(proc);
            }
        }

        built = toBuild.ToDictionary(v => v.Key.Name, v => v.Value);
    }

    public float GetWeight(Regime r, Data d)
    {
        var troops = r.GetAllTroopAmounts(d);
        var activeTroopTypes = troops.Select(kvp => kvp.Key.TroopType).ToHashSet();
        var totalPp = troops.Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
        if (totalPp == 0f) return 0f;
        var upgradePotential = 0f;
        foreach (var (troop, value) in troops)
        {
            var best = r.Military.GetBestTroopOfType(troop.TroopType, d);
            if (best != troop)
            {
                upgradePotential += (best.GetPowerPoints() - troop.GetPowerPoints()) * value;
            }
        }
        var score = Mathf.Min(10f, 100f * (upgradePotential / totalPp));
        return score;
    }


    private Dictionary<Troop, float> GetNeeded(Regime regime, Data d)
    {
        var troops = regime
            .GetAllTroopAmounts(d);
        
        var best = d.Models.GetModels<TroopType>()
            .ToDictionary(tt => tt, tt => regime.Military.GetBestTroopOfType(tt, d));

        var needed = new Dictionary<Troop, float>();
        foreach (var (troop, value) in troops)
        {
            var bestTroop = best[troop.TroopType];
            if (bestTroop != troop)
            {
                needed.Add(troop, Mathf.CeilToInt(value));
            }
        }

        return needed;
    }
}