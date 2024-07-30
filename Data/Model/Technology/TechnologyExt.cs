
using System.Linq;

public static class TechnologyExt
{
    public static bool HasPrereqs(this Regime r, ITechReqed t)
    {
        return t.Prereqs.Count == 0
               || t.Prereqs.All(tech => r.Technology.Technologies.Contains(tech.MakeRef()));
    }
    public static bool AvailableToResearch(
        this Technology t, Regime r)
    {
        return r.Technology.Technologies.Contains(t.MakeRef()) == false
               && t.Prereqs.All(p => r.Technology.Technologies.Contains(p.MakeRef()));
    }
}