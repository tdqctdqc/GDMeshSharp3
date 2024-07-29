
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Technology : IModel
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public float ResearchCost { get; private set; }
    public HashSet<Technology> Prereqs { get; private set; }
    public TechnologyCategory Category { get; private set; }
    public Technology()
    {
        
    }

    public static void AddStartingTechsForRegime(Regime r, GenKey key)
    {
        var starting = key.Data.Models.GetModels<Technology>()
            .Where(t => t.Prereqs.Count == 0)
            .Select(t => t.MakeRef());
        r.Technology.Technologies.Refs.UnionWith(starting);
    }

    public IEnumerable<IModel> GetModelsWithPrereq(Data d)
    {
        return d.Models.ModelsById.Values.OfType<ITechReqed>()
            .Where(m => m.Prereqs.Contains(this))
            .Select(m => (IModel)m);
    }
}