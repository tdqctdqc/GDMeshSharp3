using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FoodAndPopGrowthModule : LogicModule
{
    public override LogicResults Calculate(List<TurnOrders> orders, Data data)
    {
        var res = new LogicResults();
        var growthsByPeep = new Dictionary<int, int>();
        var foodConsByRegime = new Dictionary<int, int>();
        var foodConsPerPop = data.BaseDomain.Rules.FoodConsumptionPerPeepPoint;
        foreach (var regime in data.GetAll<Regime>())
        {
            var pop = regime.GetPopulation(data);
            var foodDemanded = foodConsPerPop * pop;
            var foodStock = Mathf.FloorToInt(regime.Items.Get(data.Models.Items.Food));
            var actualCons = Math.Min(foodStock, foodDemanded);
            var surplusRatio = (float) foodStock / foodDemanded - 1f;
            foodConsByRegime.Add(regime.Id, actualCons);
            if (surplusRatio > 0f)
            {
                HandleGrowth(regime, surplusRatio, growthsByPeep, data);
            }
            else
            {
                HandleDecline(regime, -surplusRatio, growthsByPeep, data);
            }
        }
        res.Messages.Add(new FoodAndPopGrowthProcedure(growthsByPeep, foodConsByRegime));
        return res;
    }

    private void HandleGrowth(Regime regime, float surplusRatio, Dictionary<int, int> growths,
        Data data)
    {
        var rules = data.BaseDomain.Rules;
        if (rules.MinSurplusRatioToGetGrowth > surplusRatio) return;
        
        var range = rules.MaxEffectiveSurplusRatio - rules.MinSurplusRatioToGetGrowth;
        if (range < 0) throw new Exception();
        
        var effectiveRatio = Mathf.Min(surplusRatio / range, rules.MaxEffectiveSurplusRatio);
        if (range < 0) throw new Exception();
        
        var peeps = regime.GetPolys(data).Where(p => p.HasPeep(data))
            .Select(p => p.GetPeep(data));
        var numPeeps = peeps.Count();
        if (numPeeps == 0) return;
        
        var effect = rules.GrowthRateCeiling * effectiveRatio * peeps.Sum(p => p.Size);
        if (effect < 0) throw new Exception();

        var numPeepsToAffect = Mathf.CeilToInt(numPeeps / 10f);
        if (numPeepsToAffect < 0) throw new Exception();
        
        var peepsToAffect = peeps.GetDistinctRandomElements(numPeepsToAffect);

        var growthPerPeep = Mathf.CeilToInt(effect / numPeepsToAffect);
        if (growthPerPeep < 0) throw new Exception();
        for (var i = 0; i < peepsToAffect.Count; i++)
        {
            growths.Add(peepsToAffect[i].Id, growthPerPeep);
        }
    }
    private void HandleDecline(Regime regime, float surplusRatio, Dictionary<int, int> growths,
        Data data)
    {

    }
}
