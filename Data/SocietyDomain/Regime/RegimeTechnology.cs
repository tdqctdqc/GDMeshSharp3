using System.Collections.Generic;
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
        Overflow = 0f;
    }

    public void SetResearch(Technology t, ProcedureWriteKey key)
    {
        if (t is not null)
        {
            CurrentResearch = t.MakeRef();
            ResearchProgresses.TryAdd(t.MakeRef(), Overflow);
            Overflow = 0f;
        }
        else
        {
            CurrentResearch = new ModelRef<Technology>();
        }
    }

    public void SetOverflow(float overflow, ProcedureWriteKey key)
    {
        Overflow = overflow;
    }
}