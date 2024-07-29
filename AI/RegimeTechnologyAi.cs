
using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeTechnologyAi
{
    private Regime _regime;
    private Dictionary<TechnologyCategory, Func<Regime, Data, float>> _getWeight;
    public RegimeTechnologyAi(Regime regime, Data d)
    {
        _regime = regime;
        _getWeight = new Dictionary<TechnologyCategory, Func<Regime, Data, float>>
        {
            {d.Models.TechnologyCategories.Economic, 
                (r,d) => 1f },
            {d.Models.TechnologyCategories.Military, 
                (r,d) => .75f },
            {d.Models.TechnologyCategories.Administrative, 
                (r,d) => .25f },
        };
    }

    public void Calculate(LogicKey key)
    {
        var weights = _getWeight.ToDictionary(kvp => kvp.Key,
            kvp => kvp.Value(_regime, key.Data));
        var techs = key.Data.Models.GetModels<Technology>();
        var researched = _regime.Technology.Technologies;
        var available = techs
            .Where(t => t.AvailableToResearch(_regime))
            .ToDictionary(t => t, t => weights[t.Category] / t.ResearchCost);

        if (available.Count == 0)
        {
            key.SendMessage(new SetResearchProcedure(_regime.MakeRef(),
                new ModelRef<Technology>()));
            return;
        }
        
        var toResearch = available
            .MaxBy(kvp => kvp.Value)
            .Key;
        key.SendMessage(new SetResearchProcedure(_regime.MakeRef(), toResearch.MakeRef()));
    }
}