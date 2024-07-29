using System.Collections.Generic;
using Godot;
using MessagePack;

public class RegimeTechnology
{
    public RefSet<ModelRef<Technology>> Technologies { get; private set; }
    public Dictionary<ModelRef<Technology>, float> Progresses { get; private set; }
    public ModelRef<Technology> Current { get; private set; }
    public float Overflow { get; private set; }
    public static RegimeTechnology Construct()
    {
        return new RegimeTechnology(
            new RefSet<ModelRef<Technology>>(new HashSet<ModelRef<Technology>>()),
            new Dictionary<ModelRef<Technology>, float>(),
            new ModelRef<Technology>(),
            0f);
    }
    
    [SerializationConstructor] private RegimeTechnology(
        RefSet<ModelRef<Technology>> technologies, 
        Dictionary<ModelRef<Technology>, float> progresses,
        ModelRef<Technology> current, 
        float overflow)
    {
        Technologies = technologies;
        Progresses = progresses;
        Overflow = overflow;
        Current = current;
    }

    public void SetResearch(ModelRef<Technology> t, ProcedureKey key)
    {
        if (Progresses.ContainsKey(t) == false)
        {
            Progresses.Add(t, 0f);
        }

        Current = t;
    }

    public void AddProgress(float progress, ProcedureKey key)
    {
        if (Current.IsEmpty())
        {
            Overflow += progress;
        }
        else
        {
            var total = Overflow + progress;
            var remaining = Current.Get(key.Data).ResearchCost - Progresses[Current];
            if (remaining <= total)
            {
                Overflow = total - remaining;
                Technologies.Add(Current, key);
                Progresses.Remove(Current);
                Current = new ModelRef<Technology>();
            }
            else
            {
                Progresses[Current] += total;
                Overflow = 0f;
            }

        }
    }
    public void SetOverflow(float overflow, ProcedureKey key)
    {
        Overflow = overflow;
    }
}