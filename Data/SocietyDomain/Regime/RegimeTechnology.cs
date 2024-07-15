using System.Collections.Generic;
using MessagePack;

public class RegimeTechnology
{
    public HashSet<ModelRef<Technology>> Technologies { get; private set; }
    public Dictionary<ModelRef<Technology>, float> ResearchProgresses { get; private set; }
    public ModelRef<Technology> CurrentResearch { get; private set; }
    public static RegimeTechnology Construct()
    {
        return new RegimeTechnology(
            new HashSet<ModelRef<Technology>>(),
            new Dictionary<ModelRef<Technology>, float>(),
            new ModelRef<Technology>());
    }
    
    [SerializationConstructor] private RegimeTechnology(
        HashSet<ModelRef<Technology>> technologies, 
        Dictionary<ModelRef<Technology>, float> researchProgresses,
        ModelRef<Technology> currentResearch)
    {
        Technologies = technologies;
        ResearchProgresses = researchProgresses;
        CurrentResearch = currentResearch;
    }
}