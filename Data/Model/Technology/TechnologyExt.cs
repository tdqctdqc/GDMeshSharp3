
using System.Linq;

public static class TechnologyExt
{
    public static bool HasPrereqs(this Regime r, ITechReqed t)
    {
        return t.Prereqs.Count == 0
               || t.Prereqs.All(tech => r.Technology.Researched.Contains(tech.MakeRef()));
    }
    public static bool AvailableToResearch(
        this Technology t, Regime r)
    {
        var ts = r.Technology.Researched;
        
        return 
            r.Technology.HaveTech(t) == false
               &&
            t.Prereqs.All(p => ts.Contains(p.MakeRef()));
    }
}