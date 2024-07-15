
using System.Collections.Generic;
using System.Linq;

public class Technology : IModel
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public float ResearchCost { get; private set; }
    public HashSet<Technology> Prereqs { get; private set; }
    public Technology()
    {
        
    }

    public static void AddStartingTechsForRegime(Regime r, GenWriteKey key)
    {
        var starting = key.Data.Models.Technologies.Models
            .Where(t => t.Prereqs.Count == 0)
            .Select(t => t.MakeRef());
        r.Technology.Technologies.UnionWith(starting);
    }
}