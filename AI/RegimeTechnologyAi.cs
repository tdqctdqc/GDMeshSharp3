
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
        var weights = _getWeight.ToDictionary(kvp => kvp.Key,
            kvp => kvp.Value(_regime, key.Data));
        var techs = key.Data.Models
            .GetModels<Technology>()
            .ToArray();

        var max = 0f;
        Technology toResearch = null;
        var researched = _regime.Technology
            .Researched
            .Select(r => r.Get(key.Data))
            .ToArray();
        
        //todo why why why 
        
        for (var j = 0; j < techs.Length; j++)
        {
            var t = techs[j];
            if (_regime.HasPrereqs(t) == false) continue;
            if (_regime.Technology.Progresses.TryGetValue(t.MakeRef(), out var p2)
                && p2 >= t.ResearchCost) continue;
            // var alreadyResearched = false;
            //
            // for (var i = 0; i < researched.Length; i++)
            // {
            //     var r = researched[i];
            //     if(r.GetHashCode().Equals(t.GetHashCode()));
            //     {
            //         alreadyResearched = true;
            //         GD.Print($"{t.Name} equals {r.Name}");
            //         break;
            //     }
            // }
            //
            // if (alreadyResearched) continue;
            GD.Print("potential " + t.Name);
            
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
        GD.Print("setting research as " + toResearch.Name);
        key.SendMessage(new SetResearchProcedure(_regime.MakeRef(), toResearch.MakeRef()));
    }
}