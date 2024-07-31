using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RegimeTechnology
{
    public IReadOnlyCollection<ModelRef<Technology>> Technologies() 
        => _techs.Select(id => new ModelRef<Technology>(id)).ToList();
    private List<int> _techs;
    
    public ModelRef<Technology> Current { get; private set; }
    public float Progress { get; private set; }
    public static RegimeTechnology Construct(Data d)
    {
        var starting = d.Models.GetModels<Technology>()
            .Where(t => t.Prereqs.Count == 0)
            .Select(t => t.Id)
            .ToList();
        return new RegimeTechnology(
            starting,
            new ModelRef<Technology>(-1),
            0f);
    }
    
    [SerializationConstructor] private RegimeTechnology(
        // HashSet<ModelRef<Technology>> technologies, 
        List<int> techs,
        ModelRef<Technology> current, 
        float progress)
    {
        // Technologies = technologies;
        _techs = techs;
        Progress = progress;
        Current = current;
    }
    
    public void SetResearch(ModelRef<Technology> t, ProcedureKey key)
    {
        Current = t;
    }

    public void AddProgress(float progress, ProcedureKey key)
    {
        Progress += progress;

        if (Current.Fulfilled())
        {
            var remaining = Current.Get(key.Data).ResearchCost - Progress;
            if (remaining <= 0f)
            {
                Progress -= remaining;
                // Technologies.Add(Current);
                _techs.Add(Current.RefId);
                Current = new ModelRef<Technology>(-1);
            }
        }
    }

    public bool HaveTech(Technology t)
    {
        return _techs.Contains(t.Id);
    }
}