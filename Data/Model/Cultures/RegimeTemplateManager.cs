using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeTemplateManager : IModelManager<RegimeTemplate>
{
    public List<RegimeTemplate> Models { get; }
    public Dictionary<string, RegimeTemplate> ByName { get; private set; }
    public RegimeTemplateManager(CultureManager cultures)
    {
        Models = cultures.Models
            .SelectMany(c => c.RegimeTemplates)
            .ToList();
        ByName = Models.ToDictionary(m => m.Name, m => m);
    }
}
