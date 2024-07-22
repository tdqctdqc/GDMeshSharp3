using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeTemplatePredefs : IModelPredefs<RegimeTemplate>
{
    public List<RegimeTemplate> RegimeTemplates { get; }
    public RegimeTemplatePredefs(CulturePredefs cultures)
    {
        RegimeTemplates = cultures.Cultures
            .SelectMany(c => c.RegimeTemplates)
            .ToList();
    }
}
