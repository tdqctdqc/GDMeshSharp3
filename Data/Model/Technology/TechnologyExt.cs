
using System.Linq;

public static class TechnologyExt
{
    public static bool HasPrereqs(this Regime r, ITechReqed t, Data d)
    {
        return t.Prereqs.Count == 0
               || t.Prereqs.All(tech => r.GetTechnologies(d).Contains(tech.MakeRef()));
    }
    public static bool AvailableToResearch(
        this Technology t, Regime r, Data d)
    {
        var techs = r.GetTechnologies(d);
        return techs.Contains(t.MakeRef()) == false
               && r.HasPrereqs(t, d);
    }
}