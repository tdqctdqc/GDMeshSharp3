using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RegimeTechnology
{
    public List<ModelRef<Technology>> Researched { get; private set; }
    public ModelRef<Technology> Current { get; private set; }
    public float Overflow { get; private set; }
    public Dictionary<ModelRef<Technology>, float> Progresses { get; private set; }
    public static RegimeTechnology Construct(Data d)
    {
        var starting = d.Models.GetModels<Technology>()
            .Where(t => t.Prereqs.Count == 0)
            .Select(t => t.MakeRef())
            .ToList();
        return new RegimeTechnology(
            starting,
            starting.ToDictionary(s => s,
                s => s.Get(d).ResearchCost),
            new ModelRef<Technology>(-1),
            0f);
    }
    
    [SerializationConstructor] private RegimeTechnology(
        List<ModelRef<Technology>> researched, 
        Dictionary<ModelRef<Technology>, float> progresses,
        ModelRef<Technology> current, 
        float overflow)
    {
        Progresses = progresses;
        Researched = researched;
        Overflow = overflow;
        Current = current;
    }
    
    public void SetResearch(ModelRef<Technology> t, ProcedureKey key)
    {
        Current = t;
        if (Progresses.ContainsKey(Current) == false
            && Current.Fulfilled())
        {
            Progresses.Add(Current, 0f);
        }
    }

    public void AddProgress(float progress, ProcedureKey key)
    {
        Overflow += progress;

        if (Current.Fulfilled())
        {
            var total = Overflow + Progresses[Current];
            var cost = Current.Get(key.Data).ResearchCost;
            var remaining = cost - total;
            if (remaining <= 0f)
            {
                Overflow = -remaining;
                Progresses[Current] += total;
                Researched.Add(Current);
                // Progresses.Remove(Current);
                Current = new ModelRef<Technology>(-1);
            }
            else
            {
                Progresses[Current] += total;
                Overflow = 0f;
            }
        }
    }

    public bool HaveTech(Technology t)
    {
        return Researched.Contains(t.MakeRef());
    }
}