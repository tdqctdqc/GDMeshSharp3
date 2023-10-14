using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeTemplateManager : IModelManager<RegimeTemplate>
{
    public Dictionary<string, RegimeTemplate> Models { get; }

    public RegimeTemplateManager(CultureManager cultures)
    {
        Models = cultures.Models
            .SelectMany(kvp => kvp.Value.RegimeTemplates)
            .ToDictionary(rt => rt.Name, rt => rt);
    }
}
