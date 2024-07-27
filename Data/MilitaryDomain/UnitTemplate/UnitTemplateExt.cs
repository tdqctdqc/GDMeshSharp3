
using System.Linq;

public static class UnitTemplateExt
{
    public static UnitMetaTemplate GetMetaTemplate
        (this UnitTemplate u, Data d)
    {
        var templatesAi = u.Regime.Get(d).GetAi(d)
            .Military.Templates;
        return templatesAi.MetaTemplates
            .FirstOrDefault(m => m.Current.Equals(u)
                        || m.Obsolete.Contains(u.MakeRef()));
    }
}