
public class DoResearchProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        var regimes = key.Data.GetAll<Regime>();
        foreach (var regime in regimes)
        {
            var tech = regime.Technology;
            var researching = tech.CurrentResearch.Get(key.Data);
            var researchPoints = regime.Stock.Stock.Get(key.Data.Models.Items.Research);
            if (researching is not null)
            {
                tech.ResearchProgresses[tech.CurrentResearch] 
                    += researchPoints;
                if (tech.ResearchProgresses[tech.CurrentResearch] >= researching.ResearchCost)
                {
                    var overflow = tech.ResearchProgresses[tech.CurrentResearch] - researching.ResearchCost;
                    tech.Technologies.Add(tech.CurrentResearch);
                    tech.ResearchProgresses.Remove(tech.CurrentResearch);
                    tech.SetResearch(null, key);
                    tech.SetOverflow(overflow, key);
                }
            }
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}