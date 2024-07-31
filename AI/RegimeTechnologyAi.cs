
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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
        if (_regime.Technology.Current.Fulfilled()) return;

        var weights = _getWeight.ToDictionary(kvp => kvp.Key,
            kvp => kvp.Value(_regime, key.Data));
        var techs = key.Data.Models
            .GetModels<Technology>()
            .ToHashSet();


        var max = 0f;
        Technology toResearch = null;
        var researched = _regime.Technology
            .Technologies().Select(t => t.Get(key.Data)).ToArray();
        
        //todo why why why 
        foreach (var t in techs)
        {
            if (_regime.HasPrereqs(t) == false) continue;
            // if (researched.Contains(t)) continue;
            var already = false;
            for (var i = 0; i < researched.Length; i++)
            {
                if(researched[i] == t);
                {
                    already = true;
                    break;
                }
            }
            
            if (already) continue;
            
            var score = weights[t.Category] / t.ResearchCost;

            if (score > max)
            {
                toResearch = t;
                max = score;
            }
        }

        if (toResearch is null)
        {
            key.SendMessage(new SetResearchProcedure(_regime.MakeRef(),
                new ModelRef<Technology>(-1)));
            return;
        }
        
        key.SendMessage(new SetResearchProcedure(_regime.MakeRef(), toResearch.MakeRef()));
    }
}