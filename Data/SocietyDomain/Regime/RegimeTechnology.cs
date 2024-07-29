using System.Collections.Generic;
using Godot;
using MessagePack;

public class RegimeTechnology
{
    public HashSet<ModelRef<Technology>> Technologies { get; private set; }
    public Dictionary<ModelRef<Technology>, float> ResearchProgresses { get; private set; }
    public ModelRef<Technology> CurrentResearch { get; private set; }
    public float Overflow { get; private set; }
    public static RegimeTechnology Construct()
    {
        return new RegimeTechnology(
            new HashSet<ModelRef<Technology>>(),
            new Dictionary<ModelRef<Technology>, float>(),
            new ModelRef<Technology>(), 0f);
    }
    
    [SerializationConstructor] private RegimeTechnology(
        HashSet<ModelRef<Technology>> technologies, 
        Dictionary<ModelRef<Technology>, float> researchProgresses,
        ModelRef<Technology> currentResearch,
        float overflow)
    {
        Technologies = technologies;
        ResearchProgresses = researchProgresses;
        CurrentResearch = currentResearch;
        Overflow = overflow;
    }

    public void SetResearch(Technology t, ProcedureKey key)
    {
        if (t is not null)
        {
            var tRef = t.MakeRef();
            CurrentResearch = tRef;
        }
        else
        {
            CurrentResearch = new ModelRef<Technology>();
        }
    }

    public void AddProgress(float progress, ProcedureKey key)
    {
        if (CurrentResearch.IsEmpty())
        {
            Overflow += progress;
        }
        else
        {
            var total = Overflow + progress;
            var curr = CurrentResearch.Get(key.Data);
            var soFar = ResearchProgresses
                .TryGetValue(CurrentResearch, out var amt)
                ? amt : 0f;
            var remaining = curr.ResearchCost - soFar;
            if (remaining <= total)
            {
                Overflow = total - remaining;
                Technologies.Add(CurrentResearch);
                ResearchProgresses.Remove(CurrentResearch);
                CurrentResearch = new ModelRef<Technology>();
            }
            else
            {
                ResearchProgresses.AddOrSum(CurrentResearch, total);
                Overflow = 0f;
            }
        }
    }
    public void SetOverflow(float overflow, ProcedureKey key)
    {
        Overflow = overflow;
    }
}