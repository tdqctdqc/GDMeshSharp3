using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeTemplateManager : IModelManager<RegimeTemplate>
{
    public List<RegimeTemplate> ExplicitModels { get; }
    public Dictionary<string, RegimeTemplate> ExplicitModelsByName { get; private set; }
    public RegimeTemplateManager(CultureManager cultures)
    {
        ExplicitModels = cultures.ExplicitModels
            .SelectMany(c => c.RegimeTemplates)
            .ToList();
        ExplicitModelsByName = ExplicitModels.ToDictionary(m => m.Name, m => m);
    }
}
