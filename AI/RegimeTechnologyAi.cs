
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeTechnologyAi
{
    private Regime _regime;
    
    public RegimeTechnologyAi(Regime regime, Data d)
    {
        _regime = regime;
    }

    public void Calculate(LogicKey key)
    {
        var weights = GetWeights(key.Data);
        var techs = key.Data.Models
            .GetModels<Technology>();
        var researched = _regime.Technology.Researched;
        var avail = new List<ModelRef<Technology>>();
        foreach (var technology in techs)
        {
            if (researched.Contains(technology.MakeRef()) == false)
            {
                avail.Add(technology.MakeRef());
            }
        }
        var max = 0f;
        Technology toResearch = null;
        
        for (var j = 0; j < techs.Count; j++)
        {
            var t = techs[j];
            if (t.AvailableToResearch(_regime) == false) continue;
            if (avail.Any(v => v.Equals(t.MakeRef())) == false) continue;
            
            var progress = _regime.Technology.Progresses
                .TryGetValue(t.MakeRef(), out var p)
                ? p
                : 0f;
            progress = Mathf.Min(progress, t.ResearchCost);
            var score = weights[t.Category] / (t.ResearchCost - progress);

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

    private Dictionary<TechnologyCategory, float> GetWeights(Data d)
    {
        return new Dictionary<TechnologyCategory, float>
        {
            { d.Models.TechnologyCategories.Economic, 1f },
            { d.Models.TechnologyCategories.Military, .75f },
            { d.Models.TechnologyCategories.Administrative, .25f },
        };
    }
}