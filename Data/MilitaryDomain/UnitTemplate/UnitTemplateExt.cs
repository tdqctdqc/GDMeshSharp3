
using System.Linq;

public static class UnitTemplateExt
{
    public static UnitMetaTemplate GetMetaTemplate
        (this UnitTemplate u, Data d)
    {
        var templatesAi = u.Regime.Get(d).GetAi(d)
            .Military.Templates;
        
        var m = templatesAi
            .MetaTemplates
            .Values
            .FirstOrDefault(m => m.Current.Equals(u)
                        || m.Obsolete.Contains(u.MakeRef()));
        if (m is null)
        {
            m = templatesAi.CategorizeTemplate(u, d);
        }

        return m;
    }
}