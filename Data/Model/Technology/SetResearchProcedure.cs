
using System.Linq;

public class SetResearchProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<Technology> Technology { get; private set; }

    public SetResearchProcedure(ERef<Regime> regime, ModelRef<Technology> technology)
    {
        Regime = regime;
        Technology = technology;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Regime.Get(key.Data).Technology
            .SetResearch(Technology.Get(key.Data), key);
    }

    public override bool Valid(Data data, out string error)
    {
        var regimeTech = Regime.Get(data).Technology;
        
        if (regimeTech.Technologies.Contains(Technology))
        {
            error = "Tech already researched";
            return false;
        }

        if (Technology.Get(data).Prereqs.Any(p => regimeTech.Technologies.Contains(p.MakeRef()) == false))
        {
            error = "Prereq techs not researched";
            return false;
        }

        error = "";
        return true;
    }
}