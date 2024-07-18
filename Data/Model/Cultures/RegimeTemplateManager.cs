using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeTemplateManager : IModelManager<RegimeTemplate>
{
    public List<RegimeTemplate> RegimeTemplates { get; }
    public RegimeTemplateManager(CultureManager cultures)
    {
        RegimeTemplates = cultures.Cultures
            .SelectMany(c => c.RegimeTemplates)
            .ToList();
    }
}
