
using System;
using System.Linq;
using Godot;

public class SetResearchProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<Technology> Technology { get; private set; }
    
    public SetResearchProcedure(ERef<Regime> regime, ModelRef<Technology> technology)
    {
        Regime = regime;
        Technology = technology;
    }

    public override void Enact(ProcedureKey key)
    {
        //todo
        // Regime.Get(key.Data).Technology
        //     .SetResearch(Technology, key);
    }

    public override bool Valid(Data data, out string error)
    {
        var regimeTechs = Regime.Get(data).GetTechnologies(data);
        
        if (regimeTechs.Contains(Technology))
        {
            error = "Tech already researched";
            return false;
        }

        if (Technology.Get(data).Prereqs
            .Any(p => regimeTechs.Contains(p.MakeRef()) == false))
        {
            error = "Prereq techs not researched";
            return false;
        }

        error = "";
        return true;
    }
}